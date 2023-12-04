#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tgs.PlayerBucket.Client;
using Tgs.PlayerService.Client;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;

internal class GetPlayerBucketsQueryService(IPlayerBucketForTypesClient playerBucketForTypesClient, IPlayerServiceClient playerServiceClient) : IQueryService<GetPlayerBucketsQuery, BucketInfo>
{
    private readonly IPlayerBucketForTypesClient _playerBucketForTypesClient = playerBucketForTypesClient ?? throw new ArgumentNullException(nameof(playerBucketForTypesClient));
    private readonly IPlayerServiceClient _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));

    public IAsyncEnumerable<BucketInfo> QueryAsync(GetPlayerBucketsQuery query, CancellationToken cancellationToken = default)
        => _playerServiceClient.GetPlayerInfoAsync(query.PlayerCode, cancellationToken)
            .AsTask()
            .ToAsyncEnumerable()
            .Where(player => player is not null)
            .SelectMany(_ => _playerBucketForTypesClient
                .GetBucketsByPlayerCodeAsync(
                    query.PlayerCode,
                    query.BucketType,
                    cancellationToken));
}
