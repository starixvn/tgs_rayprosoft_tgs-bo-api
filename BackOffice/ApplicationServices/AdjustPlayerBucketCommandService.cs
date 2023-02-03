#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerService.Client;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class AdjustPlayerBucketCommandService : ICommandService<AdjustPlayerBucketCommand>
{
    private readonly IPlayerBucketClient _playerBucketClient;
    private readonly IPlayerServiceClient _playerServiceClient;

    public AdjustPlayerBucketCommandService(IPlayerServiceClient playerServiceClient, IPlayerBucketClient playerBucketClient)
    {
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
        _playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));
    }

    public async ValueTask ExecuteAsync(AdjustPlayerBucketCommand command, CancellationToken cancellationToken = default)
    {
        if (command.AdjustAmount == 0)
            return;

        var player = await _playerServiceClient.GetPlayerInfoAsync(
            command.PlayerCode,
            cancellationToken).ConfigureAwait(false);

        if (player is null)
            return;

        if (command.AdjustAmount > 0)
            _ = await _playerBucketClient.CreateBucketAsync(
                command.PlayerCode,
                command.AdjustAmount,
                BucketType.Adjustment,
                null,
                null,
                cancellationToken).ConfigureAwait(false);
        else
            await _playerBucketClient.FillInBucketAsync(
                command.PlayerCode,
                -command.AdjustAmount,
                FillType.Adjust,
                null,
                null,
                cancellationToken).ConfigureAwait(false);
    }
}
