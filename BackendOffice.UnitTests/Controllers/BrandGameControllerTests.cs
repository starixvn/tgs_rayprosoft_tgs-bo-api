using AutoFixture;
using FluentAssertions;
using MediatR;
using NSubstitute;
using TGS.Apis.BackendOffice.Controllers.Brand;
using TGS.Services.BackOffice.Application.Models.BrandGame;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class BrandGameControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Can_Get_A_Brand_Game()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new BrandGameController(mediatorMock);

        var brandGame = CreateBrandGameResponseModelFixture().Create<BrandGameResponseModel>();

        _ = mediatorMock
            .Send(Arg.Any<GetBrandGameRequest>(), Arg.Any<CancellationToken>())
            .Returns(new GetBrandGameResponse
            {
                Valid = true,
                BrandGame = brandGame
            });

        var response = await controller.GetBrandGameAsync(Guid.NewGuid());
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

        _ = mediatorMock.Received(1).Send(Arg.Any<GetBrandGameRequest>(), Arg.Any<CancellationToken>());
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Brand_Games(bool showAll)
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new BrandGameController(mediatorMock);

        var list = CreateBrandGameModelFixture()
            .CreateMany<BrandGameModel>(99)
            .ToArray();

        var request = new GetBrandGamesRequest
        {
            Keyword = "keyword",
            Page = 1,
            Rows = 10,
            SortBy = "displayOrder",
            OrderBy = Core.Models.Messages.SortOrder.Descending,
            ShowAll = showAll
        };
        var (skip, take) = GetPage(request);
        var orderedRoles = list.OrderByDescending(p => p.DisplayOrder);

        var expectedRoles = showAll ? [.. orderedRoles] : orderedRoles.Skip(skip).Take(take).ToArray();

        _ = mediatorMock
            .Send(Arg.Any<GetBrandGamesRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResponse<BrandGameModel>
            {
                Valid = true,
                Items = expectedRoles
            });

        var response = await controller.GetBrandGamesAsync(request);
        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            _ = response.Items[i].Id.Should().Be(expectedRoles[i].Id);
            _ = response.Items[i].GameName.Should().Be(expectedRoles[i].GameName);
        }

        _ = mediatorMock.Received(1).Send(Arg.Any<GetBrandGamesRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Can_Update_A_Brand_Game()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new BrandGameController(mediatorMock);
        var brandGame = CreateBrandGameResponseModelFixture().Create<BrandGameModel>();
        var request = new Fixture().Create<UpdateBrandGameRequest>();

        _ = mediatorMock
            .Send(Arg.Any<UpdateBrandGameRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { Valid = true });

        var response = await controller.UpdateBrandGameAsync(brandGame.Id, request);

        _ = response.Valid.Should().BeTrue();
        _ = mediatorMock.Received(1).Send(Arg.Any<UpdateBrandGameRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Can_Update_A_Brand_Tag()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new BrandGameController(mediatorMock);
        var request = new Fixture().Create<UpdateBrandGameTagRequest>();

        _ = mediatorMock
            .Send(Arg.Any<UpdateBrandGameTagRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { Valid = true });

        var response = await controller.UpdateBrandTagAsync(request.Id, request.GameTag, request.Enabled);

        _ = response.Valid.Should().BeTrue();
        _ = mediatorMock.Received(1).Send(Arg.Any<UpdateBrandGameTagRequest>(), Arg.Any<CancellationToken>());
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