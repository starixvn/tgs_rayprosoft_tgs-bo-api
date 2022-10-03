using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using TGS.Services.BackOffice.Infrastructure.DataService;
using TGS.Services.WalletService;

namespace TGS.Services.BackOffice.Domain;

internal class WalletExchangeService : IWalletExchangeService
{
    private readonly IAgentService _agentService;
    private readonly IAgentWalletRepository _agentWalletRepository;
    private readonly IPlayerWalletExchangeService _playerWalletExchangeService;

    public WalletExchangeService(
        IPlayerWalletExchangeService playerWalletExchangeService,
        IAgentService agentService,
        IAgentWalletRepository agentWalletRepository)
    {
        _playerWalletExchangeService = playerWalletExchangeService ?? throw new ArgumentNullException(nameof(playerWalletExchangeService));
        _agentService = agentService ?? throw new ArgumentNullException(nameof(agentService));

        _agentWalletRepository =
            agentWalletRepository ?? throw new ArgumentNullException(nameof(agentWalletRepository));
    }

    public async ValueTask CancelWithdrawAsync(
        Guid brandId,
        UserType userType,
        string userId,
        decimal amount,
        string reviewUserId,        
        CancellationToken cancellationToken = default)
    {
        switch (userType)
        {
            case UserType.Player:
                _ = await _playerWalletExchangeService.CancelWithdrawalAsync(
                    userId,
                    amount,
                    null,
                    null,
                    string.IsNullOrWhiteSpace(reviewUserId) ? null : Guid.Parse(reviewUserId)).ConfigureAwait(false);
                return;

            case UserType.Agent:
                await ExchangeAgentWalletAsync(
                    brandId,
                    Guid.Parse(userId),
                    TransactionTypes.CancelWithdrawal,
                    amount).ConfigureAwait(false);
                return;

            default:
                throw new InvalidOperationException();
        }
    }

    public async ValueTask CommitWithdrawAsync(
        Guid brandId,
        UserType userType,
        string userId,
        decimal amount,
        decimal actualAmount,
        string reviewUserId,
        Guid formId,
        CancellationToken cancellationToken = default)
    {
        switch (userType)
        {
            case UserType.Player:
                _ = await _playerWalletExchangeService.CommitWithdrawalAsync(
                    userId,
                    amount,
                    actualAmount,
                    null,
                    formId.ToString(),
                    Guid.Parse(reviewUserId),
                    cancellationToken).ConfigureAwait(false);
                return;

            case UserType.Agent:
                return;

            default:
                throw new InvalidOperationException();
        }
    }

    public async ValueTask RollbackWithdrawAsync(
        Guid brandId,
        UserType userType,
        string userId,
        decimal amount,
        string reviewUserId,
        Guid formId,
        CancellationToken cancellationToken = default)
    {
        switch (userType)
        {
            case UserType.Player:
                _ = await _playerWalletExchangeService.RollbackWithdrawalAsync(
                    userId,
                    amount,
                    null,
                    formId.ToString(),
                    string.IsNullOrWhiteSpace(reviewUserId) ? Guid.Empty : Guid.Parse(reviewUserId),
                    cancellationToken).ConfigureAwait(false);
                return;

            case UserType.Agent:
                await ExchangeAgentWalletAsync(
                    brandId,
                    Guid.Parse(userId),
                    TransactionTypes.CancelWithdrawal,
                    amount).ConfigureAwait(false);
                return;

            default:
                throw new InvalidOperationException();
        }
    }

    private async ValueTask ExchangeAgentWalletAsync(
                        Guid brandId,
        Guid agentId,
        TransactionTypes transactionType,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        var agent = await _agentService
            .GetAgentAsync(x => x.Id == agentId)
            .ConfigureAwait(false);

        _ = await _agentWalletRepository.ExchangeAsync(
            brandId,
            agent.DefaultWalletId,
            agentId,
            transactionType,
            amount).ConfigureAwait(false);
    }
}
