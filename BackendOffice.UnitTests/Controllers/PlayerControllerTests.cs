using System.Collections.Immutable;
using System.Data.SqlClient;
using AutoFixture;
using ExpectedObjects;
using FluentAssertions;
using MediatR;
using MockQueryable.Moq;
using Moq;
using NSubstitute;
using Tgs.CashService.Client;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using Tgs.Promotion.Clients;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using TGS.Services.BackOffice.Domain;

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
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakeServiceCoordinator = Substitute.For<IServiceCoordinator>();

        var sut = new GetSinglePlayerQueryService(
            fakePlayerServiceClient,
            fakePaymentServiceClient,
            fakeCashServiceClient,
            fakePlayerBucketClient,
            fakeServiceCoordinator);

        var query = new GetSinglePlayerQuery("test1");

        var agentId = Guid.NewGuid();

        var expected = new
        {
            PlayerCode = "test1",
            AgentUsername = "agent1",
            AreaCode = "333",
            Phone = "456789",
            Balance = 500M,
            Birthdate = "2022/11/09",
            Email = "test@test.com"
        };

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(
            Arg.Is(query.PlayerCode))
            .Returns(new Tgs.PlayerService.Client.PlayerInfo
            {
                PlayerCode = query.PlayerCode,
                BrandId = Guid.NewGuid(),
                UserName = "tester",
                RegisteredOn = DateTime.UtcNow
            });

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(query.PlayerCode))
            .Returns(new Dictionary<string, string>
            {
                ["DefaultWalletId"] = Guid.NewGuid().ToString(),
                ["AgentId"] = agentId.ToString(),
                ["AreaCode"] = expected.AreaCode,
                ["Phone"] = expected.Phone,
                ["Birthdate"] = expected.Birthdate,
                ["Email"] = expected.Email
            }.ToImmutableDictionary());

        _ = fakePlayerBucketClient.ReadBucketsAsync(Arg.Is(query.PlayerCode))
            .Returns(new ReadBucketResult(1000M, Guid.NewGuid()));

        _ = fakeServiceCoordinator.QueryAsync<AllAgentsByBrandQuery, AgentInfo>(Arg.Any<AllAgentsByBrandQuery>())
            .Returns(new[] {
                new AgentInfo(
                    agentId,
                    expected.AgentUsername,
                    Guid.NewGuid(),
                    Core.Models.Enums.AgentStatus.Enabled)
            }.ToAsyncEnumerable());

        _ = fakeCashServiceClient.GetBalanceAsync(Arg.Any<Guid>())
            .Returns(500M);

        var actual = await sut.GetAsync(query);

        expected.ToExpectedObject().ShouldMatch(actual);
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

        _ = _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetPlayerOperationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResponse<PlayerOperationModel>
            {
                Valid = true,
                Items = expectedPlayers
            });

        var response = await _controller.GetPlayerOperationsAsync(playerCode, request);
        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            _ = response.Items[i].CreatedOn.Should().Be(expectedPlayers[i].CreatedOn);
            _ = response.Items[i].CreatedByUsername.Should().Be(expectedPlayers[i].CreatedByUsername);
            _ = response.Items[i].Operation.Should().Be(expectedPlayers[i].Operation);
        }

        _mediatorMock.Verify(x => x.Send(It.IsAny<GetPlayerOperationRequest>(), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Cannot_Update_Player_Not_Found()
    {
        var playerCode = Generator.RandomString(12);
        _ = _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdatePlayerRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MessageResponse
            {
                Valid = false,
                Message = Player.NotFound
            });

        var request = CreateUpdatePlayerRequestFixture().Create<Apis.BackendOffice.Controllers.Messages.Player.UpdatePlayerRequest>();

        var response = await _controller.UpdateAsync(playerCode, "info", request);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Player.NotFound);

        _mediatorMock.Verify(x => x.Send(It.IsAny<UpdatePlayerRequest>(), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Cannot_Update_Player_With_Invalid_Update_Type()
    {
        var playerCode = Generator.RandomString(12);
        _ = _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdatePlayerRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MessageResponse
            {
                Valid = false,
                Message = Player.UpdateTypeNotFound
            });

        var request = CreateUpdatePlayerRequestFixture().Create<Apis.BackendOffice.Controllers.Messages.Player.UpdatePlayerRequest>();

        var response = await _controller.UpdateAsync(playerCode, "info2", request);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Player.UpdateTypeNotFound);

        _mediatorMock.Verify(x => x.Send(It.IsAny<UpdatePlayerRequest>(), It.IsAny<CancellationToken>()));
    }

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _controller = new(
            _mediatorMock.Object,
            _promotionClientMock.Object);
    }

    private static Fixture CreateUpdatePlayerRequestFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<Apis.BackendOffice.Controllers.Messages.Player.UpdatePlayerRequest>(u => u
            .With(s => s.Birthdate, new DateOnly()));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}
