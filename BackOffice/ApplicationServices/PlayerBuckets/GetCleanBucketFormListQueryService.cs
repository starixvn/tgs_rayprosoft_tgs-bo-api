using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;

internal class GetCleanBucketFormListQueryService(IPlayerBucketClient playerBucketClient) : IQueryService<GetCleanBucketFormListQuery, CleanBucketForm>
{
    private readonly IPlayerBucketClient _playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));

    public IAsyncEnumerable<CleanBucketForm> QueryAsync(
        GetCleanBucketFormListQuery query,
        CancellationToken cancellationToken = default)
        => _playerBucketClient.QueryCleanBucketFomsAsync(
            query.PlayerCode,
            query.FlowStatus,
            cancellationToken);
}
