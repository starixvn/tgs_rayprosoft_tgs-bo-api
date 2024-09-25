using AutoFixture;
using FluentAssertions;
using MediatR;
using TGS.Apis.BackendOffice.Controllers.GameProvider;
using TGS.Services.BackOffice.Application.Models.GameProvider;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class GameProviderControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Can_Create_GameProvider()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new GameProviderController(mediatorMock);

        var request = new Fixture().Create<CreateGameProviderRequest>();

        _ = mediatorMock
            .Send(Arg.Any<CreateGameProviderRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { Valid = true });

        var response = await controller.CreateGameProviderAsync(request);
        _ = response.Valid.Should().BeTrue();

        _ = mediatorMock.Received(1).Send(Arg.Any<CreateGameProviderRequest>(), Arg.Any<CancellationToken>());
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_GameProviders(bool showAll)
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new GameProviderController(mediatorMock);

        var list = CreateGameProviderModelFixture()
            .CreateMany<GameProviderModel>(99)
            .ToArray();

        var request = new GetGameProvidersRequest
        {
            Code = "Code",
            Name = "Name",
            Page = 1,
            Rows = 10,
            SortBy = "updatedOn",
            OrderBy = TGS.Core.Models.Messages.SortOrder.Descending,
            ShowAll = showAll
        };
        var (skip, take) = GetPage(request);
        var games = list.OrderByDescending(p => p.UpdatedOn)
            .Select(g => (GameProviderBaseModel)g);

        var expectedGames = showAll ? games.ToArray() : games.Skip(skip).Take(take).ToArray();

        _ = mediatorMock
            .Send(Arg.Any<GetGameProvidersRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResponse<GameProviderBaseModel>
            {
                Valid = true,
                Items = expectedGames
            });

        var response = await controller.GetGameProvidersAsync(request);

        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            _ = response.Items[i].Id.Should().Be(expectedGames[i].Id);
            _ = response.Items[i].Name.Should().Be(expectedGames[i].Name);
        }

        _ = mediatorMock.Received(1).Send(Arg.Any<GetGameProvidersRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Can_Update_GameProvider()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new GameProviderController(mediatorMock);

        var brand = CreateGameProviderModelFixture().Create<GameProviderModel>();

        var request = new Fixture().Create<UpdateGameProviderRequest>();

        _ = mediatorMock
            .Send(Arg.Any<UpdateGameProviderRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { Valid = true });

        var response = await controller.UpdateGameProviderAsync(brand.Id, request);

        _ = response.Valid.Should().BeTrue();

        _ = mediatorMock.Received(1).Send(Arg.Any<UpdateGameProviderRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Cannot_Create_GameProvider_With_Name_Exists()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new GameProviderController(mediatorMock);

        var list = CreateGameProviderModelFixture()
            .CreateMany<GameProviderModel>(99)
            .ToArray();

        var request = new Fixture().Create<CreateGameProviderRequest>();

        request.Name = list.FirstOrDefault()?.Name;

        _ = mediatorMock
            .Send(Arg.Any<CreateGameProviderRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse
            {
                Valid = false,
                Message = GameProvider.CodeExisted
            });

        var response = await controller.CreateGameProviderAsync(request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(GameProvider.CodeExisted);

        _ = mediatorMock.Received(1).Send(Arg.Any<CreateGameProviderRequest>(), Arg.Any<CancellationToken>());
    }

    private static Fixture CreateGameProviderModelFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<GameProviderModel>(u => u
            .With(s => s.Languages, [])
            .With(s => s.Currencies, []));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}