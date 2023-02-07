using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Service.PlayerBuckets;
using TGS.Services.PlayerBucket;

#nullable enable

namespace PlayerBucket.Repositories;

public class PlayerBucketRepository : IPlayerBucketRepository
{
    private readonly IPlayerBucketClient _playerBucketClient;

    public PlayerBucketRepository(
        IPlayerBucketClient playerBucketClient)
    {
        _playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));
    }

    public async ValueTask<PlayerBucketInfo> GetPlayerBucketInfoAsync(string playerCode, CancellationToken cancellationToken = default)
    {
        var info = await _playerBucketClient.ReadBucketsAsync(
            playerCode,
            cancellationToken).ConfigureAwait(false);

        return new PlayerBucketInfo(info.Amount, info.AnchorBucketId);
    }
}
