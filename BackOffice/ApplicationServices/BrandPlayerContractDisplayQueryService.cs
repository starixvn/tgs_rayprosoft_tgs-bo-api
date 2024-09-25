using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class BrandPlayerContractDisplayQueryService()
    : IQueryService<BrandPlayerContractDisplayQuery, BrandPlayerContractDisplayInfo>
{
    public async IAsyncEnumerable<BrandPlayerContractDisplayInfo> QueryAsync(
        BrandPlayerContractDisplayQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        

        var fake = new BrandPlayerContractDisplayInfo[]
        {
            new(PlayerPropertyName.Qq, true),
            new(PlayerPropertyName.Wechat, true)
        };

        foreach (var item in fake)
        {
            yield return new BrandPlayerContractDisplayInfo(item.Name, item.IsVisualized);
        }
    }
}