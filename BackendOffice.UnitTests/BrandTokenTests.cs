using System;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.BrandCertificate.Clients;
using TGS.Services.BackOffice.Application.Queries.BrandToken;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
internal class BrandTokenTests
{
    [Test]
    public async Task Can_Get_Brand_Token()
    {
        var fakeBrandCertificateClient = Substitute.For<IBrandCertificateClient>();
        var sut = new GetBrandTokenQueryService(fakeBrandCertificateClient);
        var brandId = Guid.NewGuid();

        await sut.GetAsync(new BrandTokenQuery(brandId));

        _ = fakeBrandCertificateClient
            .Received(1)
            .MakeJsonWebTokenAsync(brandId);
    }
}
