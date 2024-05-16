using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;
using TGS.SmsService.Client;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class ListBrandSmsQueryService(ISmsServiceClient client) : IQueryService<ListBrandSmsQuery, BrandSmsInfo>
{
    public IAsyncEnumerable<BrandSmsInfo> QueryAsync(ListBrandSmsQuery query, CancellationToken cancellationToken = default)
        => client.QueryBrandSmsListAsync(query.BrandId, cancellationToken);
}