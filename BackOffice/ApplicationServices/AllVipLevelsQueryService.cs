using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;
using Tgs.VipLevelService.Clients;
using Tgs.VipLevelService.Common.ViewModels;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class AllVipLevelsQueryService(IVipLevelServiceClient vipLevelServiceClient) : IQueryService<AllVipLevelsQuery, VipLevelInfoViewModel>
{
    private readonly IVipLevelServiceClient _vipLevelServiceClient = vipLevelServiceClient ?? throw new ArgumentNullException(nameof(vipLevelServiceClient));

    public IAsyncEnumerable<VipLevelInfoViewModel> QueryAsync(AllVipLevelsQuery query, CancellationToken cancellationToken = default)
        => _vipLevelServiceClient.GetBrandAllVipLevelsAsync(query.BrandId, cancellationToken);
}
