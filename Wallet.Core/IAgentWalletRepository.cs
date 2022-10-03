using System;
using System.Threading.Tasks;

namespace TGS.Services.WalletService;

public interface IAgentWalletRepository
{
    ValueTask<(Guid TransactionId, decimal PostBalance)> ExchangeAsync(
        Guid brandId,
        Guid walletId,
        Guid agentId,
        TransactionTypes transactionType,
        decimal amount,
        string? description = null);

    ValueTask<decimal> GetBalanceAsync(Guid walletId, Guid agentId);
}
