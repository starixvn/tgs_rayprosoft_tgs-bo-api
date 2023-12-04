#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

internal class GetPlayerInfoByPlayerCodeQueryService(IPlayerServiceClient playerServiceClient) : IGetService<GetPlayerInfoByPlayerCodeQuery, PlayerInfo?>
{
    private readonly IPlayerServiceClient _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));

    public ValueTask<PlayerInfo?> GetAsync(GetPlayerInfoByPlayerCodeQuery query, CancellationToken cancellationToken = default)
        => _playerServiceClient.GetPlayerInfoAsync(query.PlayerCode, cancellationToken);
}
