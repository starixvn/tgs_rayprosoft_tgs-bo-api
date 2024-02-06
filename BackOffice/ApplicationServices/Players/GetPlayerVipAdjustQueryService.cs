using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using Tgs.VipLevelService.Clients;
using Tgs.VipLevelService.Common.ViewModels;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

internal class GetPlayerVipAdjustQueryService(IVipLevelServiceClient client) : IGetService<GetPlayerVipAdjustQuery, PagingResponseViewModel<PlayerVipLevelLogInfoViewModel>>
{
    private readonly IVipLevelServiceClient _client = client ?? throw new ArgumentNullException(nameof(client));

    public ValueTask<PagingResponseViewModel<PlayerVipLevelLogInfoViewModel>> GetAsync(GetPlayerVipAdjustQuery query, CancellationToken cancellationToken = default)
        => _client.PlayerVipLevelLogQueryAsync(
            playerCode: query.PlayerCode,
            startOn: query.From,
            endOn: query.To,
            page: query.Paging.Page,
            rowCount: query.Paging.Rows,
            cancellationToken: cancellationToken);
}
