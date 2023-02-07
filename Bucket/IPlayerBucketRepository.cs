using System;
using System.Threading;
using System.Threading.Tasks;
using TGS.Services.PlayerBucket.Models;

namespace TGS.Services.PlayerBucket;

#nullable enable

public interface IPlayerBucketRepository
{
    ValueTask<PlayerBucketInfo> GetPlayerBucketInfoAsync(string playerCode, CancellationToken cancellationToken = default);
}
