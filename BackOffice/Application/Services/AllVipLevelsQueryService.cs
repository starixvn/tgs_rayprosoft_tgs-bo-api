using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;
using Tgs.VipLevelService.Clients;

namespace TGS.Services.BackOffice.Application.Services;

internal class AllVipLevelsQueryService : IQueryService<AllVipLevelsQuery, VipLevelInfo>
{
    private readonly IVipLevelServiceClient _vipLevelServiceClient;

    public AllVipLevelsQueryService(IVipLevelServiceClient vipLevelServiceClient)
    {
        _vipLevelServiceClient = vipLevelServiceClient ?? throw new ArgumentNullException(nameof(vipLevelServiceClient));
    }

    public IAsyncEnumerable<VipLevelInfo> QueryAsync(AllVipLevelsQuery query, CancellationToken cancellationToken = default)
        => _vipLevelServiceClient.GetBrandAllVipLevelsAsync(query.BrandId, cancellationToken);
}
