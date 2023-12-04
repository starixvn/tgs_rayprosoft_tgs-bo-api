using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class AllBrandCurrenciesQueryService(IBrandServiceClient brandServiceClient) : IQueryService<AllBrandCurrenciesQuery, BrandCurrencyInfo>
{
    private readonly IBrandServiceClient _brandServiceClient = brandServiceClient ?? throw new ArgumentNullException(nameof(brandServiceClient));

    public IAsyncEnumerable<BrandCurrencyInfo> QueryAsync(
        AllBrandCurrenciesQuery query,
        CancellationToken cancellationToken = default)
        => _brandServiceClient.GetAllBrandCurrenciesAsync(cancellationToken);
}
