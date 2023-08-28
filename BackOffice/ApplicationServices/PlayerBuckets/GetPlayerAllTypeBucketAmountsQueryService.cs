using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.PlayerBucket.Client;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;

internal class GetPlayerAllTypeBucketAmountsQueryService : IQueryService<GetPlayerAllTypeBucketAmountsQuery, TypeBucketResult>
{
    private readonly IPlayerBucketForTypesClient _playerBucketForTypesClient;

    public GetPlayerAllTypeBucketAmountsQueryService(IPlayerBucketForTypesClient playerBucketForTypesClient)
    {
        _playerBucketForTypesClient = playerBucketForTypesClient ?? throw new ArgumentNullException(nameof(playerBucketForTypesClient));
    }

    public IAsyncEnumerable<TypeBucketResult> QueryAsync(GetPlayerAllTypeBucketAmountsQuery query, CancellationToken cancellationToken = default)
        => _playerBucketForTypesClient.ReadBucketsForBucketTypesAsync(
            query.PlayerCode,
            query.BucketTypes,
            cancellationToken);
}
