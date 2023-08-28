using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using Tgs.VipLevelService.Clients;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

internal class GetPlayerVipAdjustQueryService : IGetService<GetPlayerVipAdjustQuery, PagingResponse<PlayerVipAdjustInfo>>
{
    private readonly IVipLevelServiceClient _client;

    public GetPlayerVipAdjustQueryService(IVipLevelServiceClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask<PagingResponse<PlayerVipAdjustInfo>> GetAsync(GetPlayerVipAdjustQuery query, CancellationToken cancellationToken = default)
    {
        var vipAdjustRequest = new PlayerVipAdjustQueryConditions
        {
            PlayerCode = query.PlayerCode,
            BrandId = query.BrandId,
            CreateOnRange = (query.From, query.To)
        };

        var vipPaging = new Paging
        {
            Page = query.Paging.Page,
            Rows = query.Paging.Rows,
            IsDesc = query.Paging.IsDesc,
            ShowAll = query.Paging.ShowAll,
            SortBy = query.Paging.SortBy,
        };

        var response = await _client
            .QueryPlayerVipAdjustInfosAsync(vipAdjustRequest, vipPaging, cancellationToken)
            .ConfigureAwait(false);

        return response;
    }
}
