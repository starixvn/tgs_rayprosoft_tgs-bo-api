using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.DataServices.Tgs;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Models.Enums;
using TGS.Services.WalletService;

namespace TGS.Wallet.Repositories;

internal class AgentWalletRepository : IAgentWalletRepository
{
    private readonly ITgsRepository _tgsRepository;

    public AgentWalletRepository(ITgsRepository tgsRepository)
        => _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));

    public async ValueTask<(Guid TransactionId, decimal PostBalance)> ExchangeAsync(
        Guid brandId,
        Guid walletId,
        Guid agentId,
        TransactionTypes transactionType,
        decimal amount,
        string? description)
    {
        const string sql = "dbo.agent_wallet_update_wallet_balance";
        var updatedOn = DateTimeOffset.UtcNow;
        var transactionId = GuidExtensions.NewSequentialGuid();

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var postBalance = await _tgsRepository.SqlQuerySingleAsync<decimal>(
            sql,
            parameters: new Dictionary<string, object>
            {
                { ":agent_id", agentId },
                { ":wallet_id", walletId },
                { ":amount", amount },
                { ":updated_on", updatedOn }
            }).ConfigureAwait(false);

        _ = await _tgsRepository.AgentTransactions
            .AddAsync(new()
            {
                Id = transactionId,
                AgentId = agentId,
                WalletId = walletId,
                BrandId = brandId,
                Amount = amount,
                Balance = postBalance,
                Type = transactionType switch
                {
                    TransactionTypes.Deposit => AgentTransactionType.Deposit,
                    TransactionTypes.Withdrawal => AgentTransactionType.Withdrawal,
                    TransactionTypes.CompanyAdjust => AgentTransactionType.CompanyAdjust,
                    TransactionTypes.Dividend => AgentTransactionType.Dividend,
                    TransactionTypes.CancelWithdrawal => AgentTransactionType.CancelWithdrawal,
                    _ => AgentTransactionType.Other
                },
                Description = description,
                RecordedOn = DateTimeOffset.Now
            })
            .ConfigureAwait(false);

        _ = await _tgsRepository.SaveChangesAsync().ConfigureAwait(false);

        scope.Complete();

        return (transactionId, postBalance);
    }

    public async ValueTask<decimal> GetBalanceAsync(Guid walletId, Guid agentId)
    {
        var agentWallet = await _tgsRepository.AgentWallets
            .FindAsync(walletId)
            .ConfigureAwait(false);

        return agentWallet!.AgentId == agentId ? agentWallet.Balance : throw new Exception("Can't find agent wallet.");
    }
}
