using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using TGS.Apis.BackendOffice.Controllers.GameProvider;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class GameProviderBetLimitControllerTests : BaseTests
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _controller = new(
            _mediatorMock.Object);
    }

    private GameProviderBetLimitController _controller = default!;
    private readonly Mock<IMediator> _mediatorMock = new();

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_GameProviderBetLimits(bool showAll)
    {
        var list = CreateGameProviderBetLimitModelFixture()
            .CreateMany<GameProviderBetLimitModel>(99)
            .ToArray();

        var request = new GetGameProviderBetLimitsRequest { GameProviderIds = new[] { Guid.NewGuid() } };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetGameProviderBetLimitsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetGameProviderBetLimitsResponse { Valid = true, BetLimits = list });

        var response = await _controller.GetGameProviderBetLimitsAsync(request);
        response.Valid.Should().BeTrue();

        for (var i = 0; i < response.BetLimits.Length; i++)
        {
            response.BetLimits[i].Id.Should().Be(list[i].Id);
            response.BetLimits[i].MinBet.Should().Be(list[i].MinBet);
            response.BetLimits[i].MaxBet.Should().Be(list[i].MaxBet);
            response.BetLimits[i].MaxBetPerMatch.Should().Be(list[i].MaxBetPerMatch);
        }

        _mediatorMock.Verify(x =>
            x.Send(It.IsAny<GetGameProviderBetLimitsRequest>(), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Can_Create_GameProviderBetLimit()
    {
        var request = new Fixture().Create<CreateGameProviderBetLimitRequest>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateGameProviderBetLimitRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MessageResponse { Valid = true });

        var response = await _controller.CreateGameProviderBetLimitAsync(request);
        response.Valid.Should().BeTrue();

        _mediatorMock.Verify(x =>
            x.Send(It.IsAny<CreateGameProviderBetLimitRequest>(), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Can_Update_GameProviderBetLimit()
    {
        var gameProviderBetLimit = CreateGameProviderBetLimitModelFixture().Create<GameProviderBetLimitModel>();
        var request = new Fixture().Create<UpdateGameProviderBetLimitRequest>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateGameProviderBetLimitRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MessageResponse { Valid = true });

        var response = await _controller.UpdateGameProviderBetLimitAsync(gameProviderBetLimit.Id!.Value, request);

        response.Valid.Should().BeTrue();
        _mediatorMock.Verify(x =>
            x.Send(It.IsAny<UpdateGameProviderBetLimitRequest>(), It.IsAny<CancellationToken>()));
    }

    private Fixture CreateGameProviderBetLimitModelFixture()
    {
        var fixture = new Fixture();
        fixture.Create<GameProviderBetLimitModel>();
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}