using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using TGS.Apis.BackendOffice.Controllers.Brand;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.BrandGame;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class BrandGameControllerTests : BaseTests
{
    private readonly Mock<IMediator> _mediatorMock = new();

    private BrandGameController _controller = default!;

    [Test]
    public async Task Can_Get_A_Brand_Game()
    {
        var brandGame = CreateBrandGameResponseModelFixture().Create<BrandGameResponseModel>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetBrandGameRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetBrandGameResponse
            {
                Valid = true,
                BrandGame = brandGame
            });

        var response = await _controller.GetBrandGameAsync(It.IsAny<Guid>());
        response.Valid.Should().BeTrue();
        response.BrandGame.Id.Should().Be(brandGame.Id);
        response.BrandGame.GameId.Should().Be(brandGame.GameId);
        response.BrandGame.GameProviderName.Should().Be(brandGame.GameProviderName);
        response.BrandGame.GameName.Should().Be(brandGame.GameName);
        response.BrandGame.Status.Should().Be(brandGame.Status);
        response.BrandGame.GameType.Should().Be(brandGame.GameType);
        response.BrandGame.Platform.Should().Be(brandGame.Platform);
        response.BrandGame.SupportHtml5.Should().Be(brandGame.SupportHtml5);
        response.BrandGame.EffectiveBettingPercentage.Should().Be(brandGame.EffectiveBettingPercentage);
        response.BrandGame.Rebate.Should().Be(brandGame.Rebate);

        _mediatorMock.Verify(x => x.Send(It.IsAny<GetBrandGameRequest>(), It.IsAny<CancellationToken>()));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Brand_Games(bool showAll)
    {
        var list = CreateBrandGameModelFixture()
            .CreateMany<BrandGameModel>(99)
            .ToArray();

        var request = new GetBrandGamesRequest
        {
            Keyword = "keyword",
            Page = 1,
            Rows = 10,
            SortBy = "displayOrder",
            OrderBy = SortOrder.Descending,
            ShowAll = showAll
        };
        var (skip, take) = GetPage(request);
        var orderedRoles = list.OrderByDescending(p => p.DisplayOrder);

        var expectedRoles = showAll ? orderedRoles.ToArray() : orderedRoles.Skip(skip).Take(take).ToArray();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetBrandGamesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResponse<BrandGameModel>
            {
                Valid = true,
                Items = expectedRoles
            });

        var response = await _controller.GetBrandGamesAsync(request);
        response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            response.Items[i].Id.Should().Be(expectedRoles[i].Id);
            response.Items[i].GameName.Should().Be(expectedRoles[i].GameName);
        }

        _mediatorMock.Verify(x => x.Send(It.IsAny<GetBrandGamesRequest>(), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Can_Update_A_Brand_Game()
    {
        var brandGame = CreateBrandGameResponseModelFixture().Create<BrandGameModel>();
        var request = new Fixture().Create<UpdateBrandGameRequest>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateBrandGameRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MessageResponse { Valid = true });

        var response = await _controller.UpdateBrandGameAsync(brandGame.Id, request);

        response.Valid.Should().BeTrue();
        _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateBrandGameRequest>(), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Can_Update_A_Brand_Tag()
    {
        var request = new Fixture().Create<UpdateBrandGameTagRequest>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateBrandGameTagRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MessageResponse { Valid = true });

        var response = await _controller.UpdateBrandTagAsync(request.Id, request.GameTag, request.Enabled);

        response.Valid.Should().BeTrue();
        _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateBrandGameTagRequest>(), It.IsAny<CancellationToken>()));
    }

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _controller = new BrandGameController(
            _mediatorMock.Object);
    }

    private Fixture CreateBrandGameModelFixture()
    {
        var fixture = new Fixture();
        fixture.Create<BrandGameModel>();
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private Fixture CreateBrandGameResponseModelFixture()
    {
        var fixture = new Fixture();
        fixture.Create<BrandGameResponseModel>();
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}
