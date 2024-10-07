using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using Tgs.VipLevelService.Clients;
using Tgs.VipLevelService.Common.ViewModels;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

internal class GetPlayerVipAdjustQueryService(IVipLevelServiceClient client) : IGetService<GetPlayerVipAdjustQuery, PagingResponseViewModel<PlayerVipLevelLogInfoViewModel>>
{
    private readonly IVipLevelServiceClient _client = client ?? throw new ArgumentNullException(nameof(client));

    public async ValueTask<PagingResponseViewModel<PlayerVipLevelLogInfoViewModel>> GetAsync(GetPlayerVipAdjustQuery query, CancellationToken cancellationToken = default)
    {
        var playerVipLevelLogQueries = await _client.PlayerVipLevelLogQueryAsync(
             playerCode: query.PlayerCode,
             startOn: query.From,
             endOn: query.To,
             page: query.Paging.Page,
             rowCount: query.Paging.Rows,
             cancellationToken: cancellationToken)
             .ToArrayAsync(
            cancellationToken: cancellationToken)
             .ConfigureAwait(false);

        var playerVipLevelLogCount = await _client.PlayerVipLevelLogQueryCountAsync(
             playerCode: query.PlayerCode,
             startOn: query.From,
             endOn: query.To,
             cancellationToken: cancellationToken)
             .ConfigureAwait(false);

        return new PagingResponseViewModel<PlayerVipLevelLogInfoViewModel>
        {
            Page = query.Paging.Page,
            Rows = query.Paging.Rows,
            TotalCount = playerVipLevelLogCount,
            Items = playerVipLevelLogQueries.Select(pvl => new PlayerVipLevelLogInfoViewModel(
                pvl.Type,
                pvl.BeforeLevel,
                pvl.BeforeLevelId,
                pvl.BeforeAutoState,
                pvl.AfterLevel,
                pvl.AfterLevelId,
                pvl.AfterAutoState,
                pvl.CreatedOn,
                pvl.CreatedUserId,
                pvl.CreatedUsername,
                pvl.Remark))
        };
    }
}
