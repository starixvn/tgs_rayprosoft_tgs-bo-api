using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;
using TGS.Core.Models.Enums;
using Clients = Tgs.BrandCertificate.Clients;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class BrandListQueryService : IQueryService<BrandListQuery, BrandListItem>
{
    private readonly IBrandServiceClient _brandServiceClient;

    public BrandListQueryService(
        IBrandServiceClient brandServiceClient)
    {
        _brandServiceClient = brandServiceClient ?? throw new ArgumentNullException(nameof(brandServiceClient));
    }

    public IAsyncEnumerable<BrandListItem> QueryAsync(
        BrandListQuery query,
        CancellationToken cancellationToken = default)
    {
        var status = query.Status is not null
            ? query.Status switch
            {
                BrandStatus.Active => Tgs.BrandCertificate.BrandStatus.Active,
                BrandStatus.Archived => Tgs.BrandCertificate.BrandStatus.Archived,
                BrandStatus.Inactive => Tgs.BrandCertificate.BrandStatus.Inactive,
                BrandStatus.Maintenance => Tgs.BrandCertificate.BrandStatus.Maintenance,
                _ => throw new NotImplementedException()
            }
            : (Tgs.BrandCertificate.BrandStatus?)null;

        return _brandServiceClient.QueryBrandListAsync(
            new Clients.BrandListQuery
            {
                BrandIds = query.BrandIs,
                Keyword = query.Keyword,
                Status = status
            },
            cancellationToken);
    }
}
