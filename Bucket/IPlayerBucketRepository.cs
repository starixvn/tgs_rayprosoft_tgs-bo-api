using System;
using System.Threading;
using System.Threading.Tasks;
using TGS.Services.PlayerBucket.Models;

namespace TGS.Services.PlayerBucket;

#nullable enable

public interface IPlayerBucketRepository
{
    ValueTask ClearPlayerBucketsAsync(string playerCode, Guid anchorId, string? externalId, CancellationToken cancellationToken = default);

    Task CreatePlayerBucketsAsync(params CreatePlayerBucketRequest[] request);

    ValueTask<CreatePlayerBucketWithDepositResponse> CreatePlayerBucketWithDepositAsync(
        CreatePlayerBucketWithDepositRequest request);

    ValueTask<PlayerBucketInfo> GetPlayerBucketInfoAsync(string playerCode, CancellationToken cancellationToken = default);
}
