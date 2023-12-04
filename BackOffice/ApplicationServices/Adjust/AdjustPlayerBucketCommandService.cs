#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerService.Client;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Adjust;

internal class AdjustPlayerBucketCommandService(IPlayerServiceClient playerServiceClient, IPlayerBucketClient playerBucketClient) : ICommandService<AdjustPlayerBucketCommand>
{
    private readonly IPlayerBucketClient _playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));
    private readonly IPlayerServiceClient _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));

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
                command.Remark,
                cancellationToken).ConfigureAwait(false);
        else
            await _playerBucketClient.FillInBucketAsync(
                command.PlayerCode,
                -command.AdjustAmount,
                FillType.Adjust,
                null,
                command.Remark,
                cancellationToken).ConfigureAwait(false);
    }
}
