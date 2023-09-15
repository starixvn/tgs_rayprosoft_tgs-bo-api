using NSubstitute;
using Tgs.Service.PlayerBuckets;
using TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;

namespace TGS.Tests.BackendOffice.UnitTests;

public class GetBrandBucketTypesQueryServiceTests
{
    [Test]
    public async Task 取得Brand的水桶類型設定()
    {
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();

        var sut = new GetBrandBucketTypesQueryService(fakePlayerBucketClient);

        var query = new GetBrandBucketTypesQuery(Guid.NewGuid());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakePlayerBucketClient.Received(1)
            .GetBrandBucketTypesAsync(Arg.Is(query.BrandId));
    }

    [Test]
    public async Task Can_Get_Brand_Bucket_Types()
    {
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();

        var sut = new GetBrandBucketTypesQueryService(
            fakePlayerBucketClient);

        var brandId = Guid.NewGuid();

        var query = new GetBrandBucketTypesQuery(brandId);

        var bucketTypes = new[] { "SportsBook", "Lottery" };

        _ = fakePlayerBucketClient.GetBrandBucketTypesAsync(Arg.Is(brandId))
            .Returns(bucketTypes.ToAsyncEnumerable());

        var response = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakePlayerBucketClient.Received(1)
            .GetBrandBucketTypesAsync(Arg.Is(brandId));

        Assert.AreEqual(bucketTypes, response);
    }
}
