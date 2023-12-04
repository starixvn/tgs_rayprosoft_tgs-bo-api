using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.CashService.Client;
using Tgs.PlayerService.Client;

namespace TGS.Services.BackOffice.Domain;

internal class PlayerWalletExchangeService(
    IPlayerServiceClient playerServiceClient,
    ICashServiceClient cashServiceClient) : IPlayerWalletExchangeService
{
    private readonly ICashServiceClient _cashServiceClient = cashServiceClient ?? throw new ArgumentNullException(nameof(cashServiceClient));
    private readonly IPlayerServiceClient _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));

    public async ValueTask<TransactionResponse> CommitWithdrawalAsync(
        string playerCode,
        decimal amount,
        decimal actualAmount,
        string description,
        string externalId,
        Guid? createdByUserId,
        CancellationToken cancellationToken = default)
    {
        var playerProps = await _playerServiceClient
            .GetPlayerPropertiesAsync(playerCode, cancellationToken)
            .ConfigureAwait(false);

        var walletId = Guid.Parse(playerProps["DefaultWalletId"]);

        return await _cashServiceClient.CommitWithdrawAsync(
            walletId,
            amount,
            actualAmount,
            description,
            externalId,
            createdByUserId,
            cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<TransactionResponse> RollbackWithdrawalAsync(
        string playerCode,
        decimal amount,
        string description,
        string externalId,
        Guid? createdByUserId,
        CancellationToken cancellationToken = default)
    {
        var playerProps = await _playerServiceClient
            .GetPlayerPropertiesAsync(playerCode, cancellationToken)
            .ConfigureAwait(false);

        var walletId = Guid.Parse(playerProps["DefaultWalletId"]);

        return await _cashServiceClient.RollbackWithdrawAsync(
            walletId,
            amount,
            description,
            externalId,
            createdByUserId,
            cancellationToken).ConfigureAwait(false);
    }
}
