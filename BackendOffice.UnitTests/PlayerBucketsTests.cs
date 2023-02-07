using NSubstitute;
using Tgs.PlayerService.Client;
using Tgs.Service.PlayerBuckets;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.ApplicationServices.Adjust;
using TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class PlayerBucketsTests
{
    [Test]
    public async Task Can_Get_Player_Bucket_Histories()
    {
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();

        var sut = new GetPlayerBucketHistoriesQueryService(fakePlayerBucketClient);

        var query = new GetPlayerBucketHistoriesQuery(Guid.NewGuid());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakePlayerBucketClient.Received(1)
            .GetBucketEventsAsync(Arg.Is(query.BucketId));
    }

    [Test]
    public async Task Can_Get_Player_Buckets()
    {
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new GetPlayerBucketsQueryService(
            fakePlayerBucketClient,
            fakePlayerServiceClient);

        var query = new GetPlayerBucketsQuery("test1");

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(query.PlayerCode))
            .Returns(new PlayerInfo());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakePlayerBucketClient.Received(1)
            .GetBucketsByPlayerCodeAsync(Arg.Is(query.PlayerCode));
    }

    [Test]
    public async Task Get_Player_Bucket_Amount()
    {
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();

        var sut = new GetPlayerBucketAmountQueryService(fakePlayerBucketClient);

        var query = new GetPlayerBucketAmountQuery("test1");

        _ = await sut.GetAsync(query);

        _ = fakePlayerBucketClient.Received(1)
            .ReadBucketsAsync(Arg.Is(query.PlayerCode));
    }

    [Test]
    public async Task 清除流水依據PlayerCode及AnchorBucketId()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();

        var sut = new ClearPlayerBucketsCommandService(
            fakePlayerServiceClient,
            fakePlayerBucketClient);

        var command = new ClearPlayerBucketsCommand("test1", Guid.NewGuid());

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(command.PlayerCode))
            .Returns(new PlayerInfo());

        await sut.ExecuteAsync(command);

        _ = fakePlayerBucketClient.Received(1)
            .CleanBucketsAsync(
                Arg.Is(command.PlayerCode),
                Arg.Is(command.AnchorId),
                Arg.Is(default(string)),
                Arg.Any<string>());
    }

    [Test]
    public async Task 調整玩家流水_減少流水()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePlayerBucketCleint = Substitute.For<IPlayerBucketClient>();

        var sut = new AdjustPlayerBucketCommandService(
            fakePlayerServiceClient,
            fakePlayerBucketCleint);

        var command = new AdjustPlayerBucketCommand("test1", -1000M, "remark");

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(command.PlayerCode))
            .Returns(new PlayerInfo());

        await sut.ExecuteAsync(command);

        _ = fakePlayerBucketCleint.Received(1)
            .FillInBucketAsync(
                Arg.Is(command.PlayerCode),
                Arg.Is(-command.AdjustAmount),
                Arg.Is(FillType.Adjust),
                Arg.Is(default(string)),
                Arg.Is(command.Remark));
    }

    [Test]
    public async Task 調整玩家流水_增加流水()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePlayerBucketCleint = Substitute.For<IPlayerBucketClient>();

        var sut = new AdjustPlayerBucketCommandService(
            fakePlayerServiceClient,
            fakePlayerBucketCleint);

        var command = new AdjustPlayerBucketCommand("test1", 1000M, "remark");

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(command.PlayerCode))
            .Returns(new PlayerInfo());

        await sut.ExecuteAsync(command);

        _ = fakePlayerBucketCleint.Received(1)
            .CreateBucketAsync(
                Arg.Is(command.PlayerCode),
                Arg.Is(command.AdjustAmount),
                Arg.Is(BucketType.Adjustment),
                Arg.Is(default(string)),
                Arg.Is(command.Remark));
    }
}
