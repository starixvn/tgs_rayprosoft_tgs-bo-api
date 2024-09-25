using AutoFixture;
using FluentAssertions;
using MediatR;
using TGS.Apis.BackendOffice.Controllers.GameProvider;
using TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class GameProviderBetLimitControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Can_Create_GameProviderBetLimit()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new GameProviderBetLimitController(mediatorMock);

        var request = new Fixture().Create<CreateGameProviderBetLimitRequest>();

        _ = mediatorMock
            .Send(Arg.Any<CreateGameProviderBetLimitRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { Valid = true });

        var response = await controller.CreateGameProviderBetLimitAsync(request);
        _ = response.Valid.Should().BeTrue();

        _ = mediatorMock.Received(1).Send(Arg.Any<CreateGameProviderBetLimitRequest>(), Arg.Any<CancellationToken>());
    }

    public static async Task CanGetGameProviderBetLimitsAsync()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new GameProviderBetLimitController(mediatorMock);

        var list = CreateGameProviderBetLimitModelFixture()
            .CreateMany<GameProviderBetLimitModel>(99)
            .ToArray();

        var request = new GetGameProviderBetLimitsRequest
        {
            GameProviderIds = [Guid.NewGuid()]
        };

        _ = mediatorMock
            .Send(Arg.Any<GetGameProviderBetLimitsRequest>(), Arg.Any<CancellationToken>())
            .Returns(new GetGameProviderBetLimitsResponse
            {
                Valid = true,
                BetLimits = list
            });

        var response = await controller.GetGameProviderBetLimitsAsync(request);

        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.BetLimits.Length; i++)
        {
            _ = response.BetLimits[i].Id.Should().Be(list[i].Id);
            _ = response.BetLimits[i].MinBet.Should().Be(list[i].MinBet);
            _ = response.BetLimits[i].MaxBet.Should().Be(list[i].MaxBet);
            _ = response.BetLimits[i].MaxBetPerMatch.Should().Be(list[i].MaxBetPerMatch);
        }

        _ = mediatorMock.Received(1).Send(Arg.Any<GetGameProviderBetLimitsRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Can_Update_GameProviderBetLimit()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new GameProviderBetLimitController(mediatorMock);

        var gameProviderBetLimit = CreateGameProviderBetLimitModelFixture().Create<GameProviderBetLimitModel>();

        var request = new Fixture().Create<UpdateGameProviderBetLimitRequest>();

        _ = mediatorMock
            .Send(Arg.Any<UpdateGameProviderBetLimitRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { Valid = true });

        var response = await controller.UpdateGameProviderBetLimitAsync(gameProviderBetLimit.Id!.Value, request);

        _ = response.Valid.Should().BeTrue();

        _ = mediatorMock.Received(1).Send(Arg.Any<UpdateGameProviderBetLimitRequest>(), Arg.Any<CancellationToken>());
    }

    private static Fixture CreateGameProviderBetLimitModelFixture()
    {
        var fixture = new Fixture();
        _ = fixture.Create<GameProviderBetLimitModel>();
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}