#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

internal class GetPlayerInfoByPlayerCodeQueryService : IGetService<GetPlayerInfoByPlayerCodeQuery, PlayerInfo?>
{
    private readonly IPlayerServiceClient _playerServiceClient;

    public GetPlayerInfoByPlayerCodeQueryService(IPlayerServiceClient playerServiceClient)
    {
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
    }

    public ValueTask<PlayerInfo?> GetAsync(GetPlayerInfoByPlayerCodeQuery query, CancellationToken cancellationToken = default)
        => _playerServiceClient.GetPlayerInfoAsync(query.PlayerCode, cancellationToken);
}
