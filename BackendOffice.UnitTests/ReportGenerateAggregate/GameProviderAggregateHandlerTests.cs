using NSubstitute;
using NUnit.Framework.Legacy;
using StackExchange.Redis;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Redis;
using TGS.Services.BackOffice.Application.Commands.ReportGenerateAggregate;
using TGS.Services.BackOffice.Application.Models.Aggregate;
using TGS.Services.BackOffice.Infrastructure.DataService;
using Valhalla.MessageQueue;

namespace TGS.Tests.BackendOffice.UnitTests.ReportGenerateAggregate;

public class GameProviderAggregateHandlerTests
{
    [Test]
    public async Task Can_Generate_GameProvider_Aggregate()
    {
        var fakeGameProviderAggregateAuditLogService =
            Substitute.For<IGameProviderAggregateAuditLogService>();
        var fakeGameProviderService = Substitute.For<IGameProviderService>();
        var fakeMessageSender = Substitute.For<IMessageSender>();
        var fakeRedisCacheManager = Substitute.For<IRedisCacheManager>();

        var sut = new GameProviderAggregateHandler(fakeMessageSender,
            fakeGameProviderService,
            fakeGameProviderAggregateAuditLogService,
            fakeRedisCacheManager);

        var startDate = DateTimeOffset.Parse("2022/01/01 00:00:00+8");
        var gameProviderId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        var request = new GenerateGameProviderAggregateRequest(startDate, gameProviderId)
        {
            UserId = userId
        };

        _ = fakeRedisCacheManager.GetSetAllMembersAsync("aggregate", "GP").Returns(Array.Empty<RedisValue>());
        _ = fakeGameProviderService.GetGameProviderQueries().Returns(new GameProvider[]
        {
            new() { Id = gameProviderId }
        }.AsQueryable());

        var response = await sut.GetAsync(request, default);

        ClassicAssert.IsTrue(response.Valid);

        _ = fakeMessageSender.Received(1)
            .PublishAsync(Arg.Any<string>(), Arg.Any<ReadOnlyMemory<byte>>());
    }

    [Test]
    public async Task Can_Not_Generate_Again_When_The_Previous_Is_Generating()
    {
        var fakeGameProviderAggregateAuditLogService =
            Substitute.For<IGameProviderAggregateAuditLogService>();
        var fakeGameProviderService = Substitute.For<IGameProviderService>();
        var fakeMessageSender = Substitute.For<IMessageSender>();
        var fakeRedisCacheManager = Substitute.For<IRedisCacheManager>();

        var sut = new GameProviderAggregateHandler(fakeMessageSender,
            fakeGameProviderService,
            fakeGameProviderAggregateAuditLogService,
            fakeRedisCacheManager);

        var startDate = DateTimeOffset.Parse("2022/01/01 00:00:00+8");
        var gameProviderId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        var request = new GenerateGameProviderAggregateRequest(startDate, gameProviderId)
        {
            UserId = userId
        };

        _ = fakeRedisCacheManager.GetSetAllMembersAsync("aggregate", "GP").Returns(new RedisValue[]
        { new(gameProviderId.ToString())
        });

        _ = fakeGameProviderService.GetGameProviderQueries().Returns(new GameProvider[]
        {
            new() { Id = gameProviderId }
        }.AsQueryable());

        var response = await sut.GetAsync(request, default);

        ClassicAssert.IsFalse(response.Valid);

        _ = fakeMessageSender.Received(0)
            .PublishAsync(Arg.Any<string>(), Arg.Any<ReadOnlyMemory<byte>>());
    }

    [Test]
    public async Task Can_Not_Generate_GameProvider_Aggregate_When_Not_Number_One_Of_Month()
    {
        var fakeGameProviderAggregateAuditLogService =
            Substitute.For<IGameProviderAggregateAuditLogService>();
        var fakeGameProviderService = Substitute.For<IGameProviderService>();
        var fakeMessageSender = Substitute.For<IMessageSender>();
        var fakeRedisCacheManager = Substitute.For<IRedisCacheManager>();

        var sut = new GameProviderAggregateHandler(fakeMessageSender,
            fakeGameProviderService,
            fakeGameProviderAggregateAuditLogService,
            fakeRedisCacheManager);

        var startDate = DateTimeOffset.Parse("2022/01/02 00:00:00+8");
        var gameProviderId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        var request = new GenerateGameProviderAggregateRequest(startDate, gameProviderId)
        {
            UserId = userId
        };

        var response = await sut.GetAsync(request, default);

        ClassicAssert.IsFalse(response.Valid);

        _ = fakeMessageSender.Received(0)
            .PublishAsync(Arg.Any<string>(), Arg.Any<ReadOnlyMemory<byte>>());
    }
}
