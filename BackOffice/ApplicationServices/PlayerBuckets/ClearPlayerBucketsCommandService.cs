#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerService.Client;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;

internal class ClearPlayerBucketsCommandService : ICommandService<ClearPlayerBucketsCommand>
{
    private readonly IPlayerBucketClient _playerBucketClient;
    private readonly IPlayerServiceClient _playerServiceClient;

    public ClearPlayerBucketsCommandService(IPlayerServiceClient playerServiceClient, IPlayerBucketClient playerBucketClient)
    {
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
        _playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));
    }

    public async ValueTask ExecuteAsync(ClearPlayerBucketsCommand command, CancellationToken cancellationToken = default)
    {
        var player = await _playerServiceClient.GetPlayerInfoAsync(
            command.PlayerCode,
            cancellationToken).ConfigureAwait(false);

        if (player is null)
            return;

        await _playerBucketClient.CleanBucketsAsync(
            command.PlayerCode,
            command.AnchorId,
            null,
            null,
            cancellationToken).ConfigureAwait(false);
    }
}
