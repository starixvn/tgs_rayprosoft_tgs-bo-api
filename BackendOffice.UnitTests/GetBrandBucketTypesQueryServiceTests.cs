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
}
