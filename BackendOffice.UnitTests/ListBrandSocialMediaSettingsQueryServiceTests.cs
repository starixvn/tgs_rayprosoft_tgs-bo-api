using Tgs.BrandCertificate.Clients;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Tests.BackendOffice.UnitTests;

public class ListBrandSocialMediaSettingsQueryServiceTests
{
    [Test]
    public async Task ListBrandSocialMediaSettingsQueryService_取得社群連結列表()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

        var sut = new ListBrandSocialMediaSettingsQueryService(fakeBrandServiceClient);

        var query = new ListBrandSocialMediaSettingsQuery(Guid.NewGuid());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakeBrandServiceClient.Received(1)
            .ListSocialMediaSettingsAsync(Arg.Is(query.BrandId));
    }
}
