using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class GetPlayerBucketsQueryService : IQueryService<GetPlayerBucketsQuery, BucketInfo>
{
    private readonly IPlayerBucketClient _playerBucketClient;

    public GetPlayerBucketsQueryService(IPlayerBucketClient playerBucketClient)
    {
        _playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));
    }

    public IAsyncEnumerable<BucketInfo> QueryAsync(GetPlayerBucketsQuery query, CancellationToken cancellationToken = default)
        => _playerBucketClient.GetBucketsByPlayerCodeAsync(query.PlayerCode, cancellationToken);
}
