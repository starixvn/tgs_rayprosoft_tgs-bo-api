using Tgs.PlayerBucket.Client;
using TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;

namespace TGS.Tests.BackendOffice.UnitTests;

public class GetPlayerAllTypeBucketAmountsQueryServiceTests
{
    [Test]
    public async Task 取得玩家流水水桶含遊戲類型()
    {
        var fakePlayerBucketForTypesClient = Substitute.For<IPlayerBucketForTypesClient>();

        var sut = new GetPlayerAllTypeBucketAmountsQueryService(fakePlayerBucketForTypesClient);

        var bucketTypes = new[] { "Game1", "Game2" };
        var query = new GetPlayerAllTypeBucketAmountsQuery(
            "test1",
            bucketTypes);

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakePlayerBucketForTypesClient.Received(1)
            .ReadBucketsForBucketTypesAsync(Arg.Is(query.PlayerCode), Arg.Is(query.BucketTypes));
    }
}
