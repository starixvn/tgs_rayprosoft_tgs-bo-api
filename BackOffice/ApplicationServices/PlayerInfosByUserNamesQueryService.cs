using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class PlayerInfosByUserNamesQueryService : IQueryService<PlayerInfosByUserNamesQuery, PlayerInfo>
{
    private readonly IPlayerServiceClient _playerServiceClient;

    public PlayerInfosByUserNamesQueryService(
        IPlayerServiceClient playerServiceClient)
    {
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
    }

    public async IAsyncEnumerable<PlayerInfo> QueryAsync(
        PlayerInfosByUserNamesQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var playerCodes = await _playerServiceClient
            .GetPlayerCodesByUserNamesAsync(query.BrandId, query.UserNames, cancellationToken)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        if (playerCodes.Length == 0)
            yield break;

        var playerInfos = await _playerServiceClient
            .QueryPlayerInfoListAsync(query.BrandId, playerCodes, cancellationToken)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var info in playerInfos)
            yield return info;
    }
}