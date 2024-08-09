using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class BrandPlayerContractDisplayQueryService(IBrandServiceClient brandServiceClient)
    : IQueryService<BrandPlayerContractDisplayQuery, BrandPlayerContractDisplayInfo>
{
    public async IAsyncEnumerable<BrandPlayerContractDisplayInfo> QueryAsync(
        BrandPlayerContractDisplayQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var displayInfo = await brandServiceClient
            .ListBrandPlayerContractDisplayAsync(query.BrandId, cancellationToken)
            .ToArrayAsync(cancellationToken);

        foreach (var item in displayInfo)
        {
            yield return new BrandPlayerContractDisplayInfo
            {
                Name = item.Name,
                IsVisualized = item.IsVisualized,
            };
        }
    }
}