using Tgs.BrandCertificate.Clients;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Tests.BackendOffice.UnitTests;

public class BrandPlayerContractDisplayQueryServiceTests
{
    [Test]
    public async Task BrandPlayerContractDisplayQueryService_取得平台聯繫信息設定()
    {
        var fakeClient = Substitute.For<IBrandServiceClient>();

        var sut = new BrandPlayerContractDisplayQueryService(fakeClient);

        var brandId = Guid.NewGuid();
        _ = await sut.QueryAsync(new BrandPlayerContractDisplayQuery(brandId)).ToArrayAsync();

        _ = fakeClient.Received(1)
            .ListBrandPlayerContractDisplayAsync(Arg.Is(brandId));
    }
}