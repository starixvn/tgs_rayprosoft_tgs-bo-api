using System;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.Service.PlayerBuckets;
using TGS.Services.BackOffice.ApplicationServices;

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

        var sut = new GetPlayerBucketsQueryService(fakePlayerBucketClient);

        var query = new GetPlayerBucketsQuery("test1");

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
}
