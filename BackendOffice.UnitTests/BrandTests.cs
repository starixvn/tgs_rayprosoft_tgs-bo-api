using NSubstitute;
using Tgs.BrandCertificate.Clients;
using TGS.Services.BackOffice.ApplicationServices;
using BrandListQuery = TGS.Services.BackOffice.ApplicationServices.BrandListQuery;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
internal class BrandTests
{
    [Test]
    public async Task 取得BrandInfo()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

        var sut = new GetBrandInfoQueryService(fakeBrandServiceClient);

        var query = new GetBrandInfoQuery(Guid.NewGuid());

        _ = await sut.GetAsync(query);

        _ = fakeBrandServiceClient.Received(1)
            .GetBrandInfoAsync(
                Arg.Is(query.BrandId),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 取得Brand關聯表()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

        var sut = new BrandRelationQueryService(fakeBrandServiceClient);

        var query = new BrandRelationQuery();

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakeBrandServiceClient.Received(1)
            .GetBrandRelationsAsync(
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 取得Brand列表()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

        var sut = new BrandListQueryService(fakeBrandServiceClient);

        var query = new BrandListQuery(
            Array.Empty<Guid>(),
            null,
            null);

        _ = await sut.QueryAsync(query).ToArrayAsync();
        
        _ = fakeBrandServiceClient.Received(1)
            .QueryBrandListAsync(
                Arg.Is<Tgs.BrandCertificate.Clients.BrandListQuery>(x => 
                    x.BrandIds.SequenceEqual(query.BrandIs)),
                Arg.Any<CancellationToken>());
    }
}
