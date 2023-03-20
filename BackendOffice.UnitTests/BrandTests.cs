using NSubstitute;
using Tgs.BrandCertificate.Clients;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Tests.BackendOffice.UnitTests;


[TestFixture]
internal class BrandTests
{
    [Test]
    public async Task 取得BrandInfo()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

        var query = new GetBrandInfoQuery(Guid.NewGuid());

        var sut = new GetBrandInfoQueryService(fakeBrandServiceClient);

        _ = await sut.GetAsync(query);

        _ = fakeBrandServiceClient.Received(1)
            .GetBrandInfoAsync(
                Arg.Is(query.BrandId),
                Arg.Any<CancellationToken>());
    }
}