using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.CashService.Client;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Adjust;

internal class AdjustPlayerLockedAmountCommandService(
    ICashServiceClient cashServiceClient,
    IPlayerServiceClient playerServiceClient)
    : ICommandService<AdjustPlayerLockedAmountCommand>
{
    private readonly ICashServiceClient _cashServiceClient = cashServiceClient ?? throw new ArgumentNullException(nameof(cashServiceClient));
    private readonly IPlayerServiceClient _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));

    public async ValueTask ExecuteAsync(AdjustPlayerLockedAmountCommand command, CancellationToken cancellationToken = default)
    {
        var playerInfo = await _playerServiceClient
            .GetPlayerInfoAsync(command.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        if (playerInfo is null)
            return;

        var props = await _playerServiceClient
            .GetPlayerPropertiesAsync(command.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        if (!props.TryGetValue("DefaultWalletId", out var walletId))
            return;

        _ = await _cashServiceClient.AdjustSystemV2Async(
            Guid.Parse(walletId),
            0M,
            command.LockAmount,
            command.Remark,
            null,
            command.ModifyUserId,
            cancellationToken).ConfigureAwait(false);
    }
}
