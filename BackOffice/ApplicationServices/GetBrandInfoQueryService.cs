#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class GetBrandInfoQueryService(
    IBrandServiceClient brandServiceClient) : IGetService<GetBrandInfoQuery, BrandInfo?>
{
    private readonly IBrandServiceClient _brandServiceClient = brandServiceClient ?? throw new ArgumentNullException(nameof(brandServiceClient));

    public ValueTask<BrandInfo?> GetAsync(
        GetBrandInfoQuery query, 
        CancellationToken cancellationToken = default) 
        => _brandServiceClient.GetBrandInfoAsync(query.BrandId, cancellationToken);
}