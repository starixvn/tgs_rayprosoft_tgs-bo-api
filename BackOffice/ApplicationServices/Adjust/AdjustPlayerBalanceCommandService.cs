using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Adjust;

internal class AdjustPlayerBalanceCommandService : ICommandService<AdjustPlayerBalanceCommand>
{
    private readonly IPlayerAdjustBalanceService _playerAdjustBalanceService;
    private readonly IPlayerServiceClient _playerServiceClient;

    public AdjustPlayerBalanceCommandService(
        IPlayerServiceClient playerServiceClient,
        IPlayerAdjustBalanceService playerAdjustBalanceService)
    {
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
        _playerAdjustBalanceService = playerAdjustBalanceService ?? throw new ArgumentNullException(nameof(playerAdjustBalanceService));
    }

    public async ValueTask ExecuteAsync(AdjustPlayerBalanceCommand command, CancellationToken cancellationToken = default)
    {
        var playerInfo = await _playerServiceClient
            .GetPlayerInfoAsync(command.PlayerCode, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new Exception(Player.NotFound);

        var props = await _playerServiceClient
            .GetPlayerPropertiesAsync(command.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        if (!props.TryGetValue("DefaultWalletId", out var walletId))
            throw new Exception(Agent.WalletNotFound);

        var applyInfo = new PlayerBalanceAdjustmentApplyInfo(
            playerInfo.BrandId,
            playerInfo.PlayerCode,
            Guid.Parse(walletId),
            command.AdjustmentType,
            command.PlayerAdjustmentTransactionType,
            AdjustTransferType.None,
            command.Amount,
            command.BucketAmount,
            command.IncludeInNetProfit,
            command.Remark);

        await _playerAdjustBalanceService.ProcessAdjustApplyAsync(
            applyInfo,
            command.ModifyUserId,
            command.ModifyUser,
            cancellationToken).ConfigureAwait(false);
    }
}
