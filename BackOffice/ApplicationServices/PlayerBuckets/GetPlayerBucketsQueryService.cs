#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tgs.PlayerService.Client;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;

internal class GetPlayerBucketsQueryService : IQueryService<GetPlayerBucketsQuery, BucketInfo>
{
    private readonly IPlayerBucketClient _playerBucketClient;
    private readonly IPlayerServiceClient _playerServiceClient;

    public GetPlayerBucketsQueryService(IPlayerBucketClient playerBucketClient, IPlayerServiceClient playerServiceClient)
    {
        _playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
    }

    public IAsyncEnumerable<BucketInfo> QueryAsync(GetPlayerBucketsQuery query, CancellationToken cancellationToken = default)
        => _playerServiceClient.GetPlayerInfoAsync(query.PlayerCode, cancellationToken)
            .AsTask()
            .ToAsyncEnumerable()
            .Where(player => player is not null)
            .SelectMany(_ => _playerBucketClient
                .GetBucketsByPlayerCodeAsync(query.PlayerCode, cancellationToken));
}
