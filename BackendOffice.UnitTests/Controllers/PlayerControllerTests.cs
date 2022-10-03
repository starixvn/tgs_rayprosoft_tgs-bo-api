using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using Tgs.Promotion.Clients;
using TGS.Apis.BackendOffice.Controllers;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Application.Resources;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class PlayerControllerTests : BaseTests
{
    private readonly Mock<IMediator> _mediatorMock = new();

    private readonly Mock<IPromotionClient> _promotionClientMock = new();

    private PlayerController _controller = default!;

    [Test]
    public async Task Can_Get_Player()
    {
        var fixture = new Fixture();
        var player = CreatePlayersModelFixture().Create<PlayerModel>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetPlayerRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPlayerResponse
            {
                Valid = true,
                Player = player
            });

        var response = await _controller.GetSingleAsync(It.IsAny<string>());

        response.Valid.Should().BeTrue();
        response.Player.Status.Should().Be(player.Status);
        response.Player.Username.Should().Be(player.Username);
        response.Player.RealName.Should().Be(player.RealName);
        response.Player.RegisteredOn.Should().Be(player.RegisteredOn);
        response.Player.AgentUsername.Should().Be(player.AgentUsername);
        response.Player.Balance.Should().Be(player.Balance);
        response.Player.Phone.Should().Be(player.Phone);
        response.Player.Email.Should().Be(player.Email);
        response.Player.QQ.Should().Be(player.QQ);
        response.Player.WeChat.Should().Be(player.WeChat);
        response.Player.FacebookMessenger.Should().Be(player.FacebookMessenger);
        response.Player.Viber.Should().Be(player.Viber);
        response.Player.Line.Should().Be(player.Line);
        response.Player.Telegram.Should().Be(player.Telegram);
        response.Player.WhatsApp.Should().Be(player.WhatsApp);

        _mediatorMock.Verify(x => x.Send(It.IsAny<GetPlayerRequest>(), It.IsAny<CancellationToken>()));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Player_Operations(bool showAll)
    {
        var playerCode = Generator.RandomString(12);

        var request = new GetPlayerOperationRequest
        {
            Page = 1,
            Rows = 10,
            SortBy = "CreatedOn",
            OrderBy = SortOrder.Descending,
            ShowAll = showAll
        };

        var fixture = new Fixture();
        var list = fixture
            .CreateMany<PlayerOperationModel>(105)
            .AsQueryable()
            .BuildMock();

        var (skip, take) = GetPage(request);
        var orderedPlayers = list.OrderByDescending(p => p.CreatedOn);

        var expectedPlayers = showAll ? orderedPlayers.ToArray() : orderedPlayers.Skip(skip).Take(take).ToArray();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetPlayerOperationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResponse<PlayerOperationModel>
            {
                Valid = true,
                Items = expectedPlayers
            });

        var response = await _controller.GetPlayerOperationsAsync(playerCode, request);
        response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            response.Items[i].CreatedOn.Should().Be(expectedPlayers[i].CreatedOn);
            response.Items[i].CreatedByUsername.Should().Be(expectedPlayers[i].CreatedByUsername);
            response.Items[i].Operation.Should().Be(expectedPlayers[i].Operation);
        }

        _mediatorMock.Verify(x => x.Send(It.IsAny<GetPlayerOperationRequest>(), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Cannot_Get_Player_Not_Found()
    {
        var playerCode = Generator.RandomString(12);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetPlayerRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPlayerResponse
            {
                Valid = false,
                Message = Player.NotFound
            });

        var response = await _controller.GetSingleAsync(playerCode);
        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Player.NotFound);

        _mediatorMock.Verify(x => x.Send(It.IsAny<GetPlayerRequest>(), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Cannot_Update_Player_Not_Found()
    {
        var playerCode = Generator.RandomString(12);
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdatePlayerRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MessageResponse
            {
                Valid = false,
                Message = Player.NotFound
            });

        var request = CreateUpdatePlayerRequestFixture().Create<Apis.BackendOffice.Controllers.Messages.Player.UpdatePlayerRequest>();

        var response = await _controller.UpdateAsync(playerCode, "info", request);

        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Player.NotFound);

        _mediatorMock.Verify(x => x.Send(It.IsAny<UpdatePlayerRequest>(), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Cannot_Update_Player_With_Invalid_Update_Type()
    {
        var playerCode = Generator.RandomString(12);
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdatePlayerRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MessageResponse
            {
                Valid = false,
                Message = Player.UpdateTypeNotFound
            });

        var request = CreateUpdatePlayerRequestFixture().Create<Apis.BackendOffice.Controllers.Messages.Player.UpdatePlayerRequest>();

        var response = await _controller.UpdateAsync(playerCode, "info2", request);

        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Player.UpdateTypeNotFound);

        _mediatorMock.Verify(x => x.Send(It.IsAny<UpdatePlayerRequest>(), It.IsAny<CancellationToken>()));
    }

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _controller = new(
            _mediatorMock.Object,
            _promotionClientMock.Object,
            NullLogger<PlayerController>.Instance);
    }

    private Fixture CreatePlayersModelFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerModel>(u => u
            .With(s => s.Banks, new PlayerModel.BankModel[]
            {
            }));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private Fixture CreateUpdatePlayerRequestFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<Apis.BackendOffice.Controllers.Messages.Player.UpdatePlayerRequest>(u => u
            .With(s => s.Birthdate, new DateOnly()));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}
