using System.Data.SqlClient;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Moq;
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

        _ = _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetBrandGameRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetBrandGameResponse
            {
                Valid = true,
                BrandGame = brandGame
            });

        var response = await _controller.GetBrandGameAsync(It.IsAny<Guid>());
        _ = response.Valid.Should().BeTrue();
        _ = response.BrandGame.Id.Should().Be(brandGame.Id);
        _ = response.BrandGame.GameId.Should().Be(brandGame.GameId);
        _ = response.BrandGame.GameProviderName.Should().Be(brandGame.GameProviderName);
        _ = response.BrandGame.GameName.Should().Be(brandGame.GameName);
        _ = response.BrandGame.Status.Should().Be(brandGame.Status);
        _ = response.BrandGame.GameType.Should().Be(brandGame.GameType);
        _ = response.BrandGame.Platform.Should().Be(brandGame.Platform);
        _ = response.BrandGame.SupportHtml5.Should().Be(brandGame.SupportHtml5);
        _ = response.BrandGame.EffectiveBettingPercentage.Should().Be(brandGame.EffectiveBettingPercentage);
        _ = response.BrandGame.Rebate.Should().Be(brandGame.Rebate);

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

        _ = _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetBrandGamesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResponse<BrandGameModel>
            {
                Valid = true,
                Items = expectedRoles
            });

        var response = await _controller.GetBrandGamesAsync(request);
        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            _ = response.Items[i].Id.Should().Be(expectedRoles[i].Id);
            _ = response.Items[i].GameName.Should().Be(expectedRoles[i].GameName);
        }

        _mediatorMock.Verify(x => x.Send(It.IsAny<GetBrandGamesRequest>(), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Can_Update_A_Brand_Game()
    {
        var brandGame = CreateBrandGameResponseModelFixture().Create<BrandGameModel>();
        var request = new Fixture().Create<UpdateBrandGameRequest>();

        _ = _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateBrandGameRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MessageResponse { Valid = true });

        var response = await _controller.UpdateBrandGameAsync(brandGame.Id, request);

        _ = response.Valid.Should().BeTrue();
        _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateBrandGameRequest>(), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Can_Update_A_Brand_Tag()
    {
        var request = new Fixture().Create<UpdateBrandGameTagRequest>();

        _ = _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateBrandGameTagRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MessageResponse { Valid = true });

        var response = await _controller.UpdateBrandTagAsync(request.Id, request.GameTag, request.Enabled);

        _ = response.Valid.Should().BeTrue();
        _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateBrandGameTagRequest>(), It.IsAny<CancellationToken>()));
    }

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _controller = new BrandGameController(
            _mediatorMock.Object);
    }

    private static Fixture CreateBrandGameModelFixture()
    {
        var fixture = new Fixture();
        _ = fixture.Create<BrandGameModel>();
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreateBrandGameResponseModelFixture()
    {
        var fixture = new Fixture();
        _ = fixture.Create<BrandGameResponseModel>();
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}
