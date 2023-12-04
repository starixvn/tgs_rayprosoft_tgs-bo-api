using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;

internal class GetPlayerBucketHistoriesQueryService(IPlayerBucketClient playerBucketClient) : IQueryService<GetPlayerBucketHistoriesQuery, BucketEvent>
{
    private readonly IPlayerBucketClient _playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));

    public IAsyncEnumerable<BucketEvent> QueryAsync(GetPlayerBucketHistoriesQuery query, CancellationToken cancellationToken = default)
        => _playerBucketClient.GetBucketEventsAsync(query.BucketId, cancellationToken);
}
