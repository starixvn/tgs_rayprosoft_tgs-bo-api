using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class BrandRelationQueryService : IQueryService<BrandRelationQuery, BrandRelationInfo>
{
    private readonly IBrandServiceClient _brandServiceClient;

    public BrandRelationQueryService(
        IBrandServiceClient brandServiceClient)
    {
        _brandServiceClient = brandServiceClient ?? throw new ArgumentNullException(nameof(brandServiceClient));
    }

    public IAsyncEnumerable<BrandRelationInfo> QueryAsync(
        BrandRelationQuery query, 
        CancellationToken cancellationToken = default) 
        => _brandServiceClient.GetBrandRelationsAsync(cancellationToken);
}