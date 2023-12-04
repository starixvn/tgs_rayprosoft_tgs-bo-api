using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

internal class PlayerHistoryQueryService(IPlayerServiceClient playerServiceClient) : IGetService<PlayerHistoryQuery, PlayerHistoryEventInfo>
{
    private readonly IPlayerServiceClient _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));

    public async ValueTask<PlayerHistoryEventInfo> GetAsync(PlayerHistoryQuery query, CancellationToken cancellationToken = default)
    {
        var info = await _playerServiceClient
            .GetPlayerHistoryAsync(query.EventId, cancellationToken)
            .ConfigureAwait(false);

        return new PlayerHistoryEventInfo(info.Action, info.ChangeSets, info.CreatedOn, info.Modifier);
    }
}
