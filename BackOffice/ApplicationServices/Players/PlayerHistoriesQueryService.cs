using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

internal class PlayerHistoriesQueryService(IPlayerServiceClient playerServiceClient) : IQueryService<PlayerHistoriesQuery, PlayerHistoryInfo>, IGetService<PlayerHistoriesQuery, int>
{
    private readonly IPlayerServiceClient _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));

    public ValueTask<int> GetAsync(PlayerHistoriesQuery query, CancellationToken cancellationToken = default)
        => _playerServiceClient.CountPlayerHistoriesAsync(
            new GetPlayerHistoriesQueryConditions(query.PlayerCode, query.From, query.To),
            cancellationToken);

    public IAsyncEnumerable<PlayerHistoryInfo> QueryAsync(PlayerHistoriesQuery query, CancellationToken cancellationToken = default)
        => AsyncEnumerableEx.Return(new GetPlayerHistoriesQueryConditions(query.PlayerCode, query.From, query.To))
            .SelectMany(condition => _playerServiceClient.CountPlayerHistoriesAsync(
                condition,
                cancellationToken)
                .AsTask()
                .ToAsyncEnumerable()
                .SelectMany(count => _playerServiceClient.GetPlayerHistoriesAsync(
                    condition,
                    count,
                    query.Skip,
                    cancellationToken)));
}
