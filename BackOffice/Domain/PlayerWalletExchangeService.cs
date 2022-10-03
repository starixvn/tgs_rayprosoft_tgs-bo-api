using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.CashService.Client;
using Tgs.PlayerService.Client;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Domain;

internal class PlayerWalletExchangeService : IPlayerWalletExchangeService
{
    private readonly ICashServiceClient _cashServiceClient;
    private readonly IPlayerService _playerService;
    private readonly IPlayerServiceClient _playerServiceClient;

    public PlayerWalletExchangeService(
        IPlayerService playerService,
        IPlayerServiceClient playerServiceClient,
        ICashServiceClient cashServiceClient)
    {
        _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
        _cashServiceClient = cashServiceClient ?? throw new ArgumentNullException(nameof(cashServiceClient));
    }

    public async ValueTask<TransactionResponse> CancelWithdrawalAsync(
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

        return await _cashServiceClient.CancelWithdrawalAsync(
            walletId,
            amount,
            description,
            externalId,
            createdByUserId,
            cancellationToken).ConfigureAwait(false);
    }

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
