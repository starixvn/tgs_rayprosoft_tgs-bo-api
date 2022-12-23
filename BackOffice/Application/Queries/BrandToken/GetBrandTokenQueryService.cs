using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.BrandToken;

internal class GetBrandTokenQueryService : IGetService<BrandTokenQuery, string>
{
    private readonly IBrandCertificateClient _brandCertificateClient;

    public GetBrandTokenQueryService(IBrandCertificateClient brandCertificateClient)
    {
        _brandCertificateClient = brandCertificateClient ?? throw new ArgumentNullException(nameof(brandCertificateClient));
    }

    public async ValueTask<string> GetAsync(BrandTokenQuery query, CancellationToken cancellationToken = default)
        => await _brandCertificateClient
            .MakeJsonWebTokenAsync(query.BrandId, cancellationToken)
            .ConfigureAwait(false);
}
