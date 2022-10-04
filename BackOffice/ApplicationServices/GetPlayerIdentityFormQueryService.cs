using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class GetPlayerIdentityFormQueryService : IGetService<GetPlayerIdentityFormQuery, PlayerIdentityInfo>
{
    private readonly IPlayerService _playerService;

    public GetPlayerIdentityFormQueryService(IPlayerService playerService)
    {
        _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
    }

    public async ValueTask<PlayerIdentityInfo> GetAsync(GetPlayerIdentityFormQuery query, CancellationToken cancellationToken = default)
    {
        var formInfo = await _playerService
            .GetPlayerIdentityFormAsync(query.PlayerCode)
            .ConfigureAwait(false);

        return new PlayerIdentityInfo(
            formInfo.Id,
            formInfo.RealName,
            formInfo.IdNumber,
            formInfo.Data,
            formInfo.CreatedOn,
            formInfo.Status,
            formInfo.Remark,
            formInfo.ReviewedOn,
            formInfo.ReviewedUserName);
    }
}
