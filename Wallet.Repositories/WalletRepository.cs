using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.DataServices.Tgs;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Models.Enums;
using TGS.Services.WalletService;

namespace TGS.Wallet.Repositories
{
    internal class WalletRepository : IWalletRepository
    {
        private readonly ITgsRepository _tgsRepository;

        public WalletRepository(ITgsRepository tgsRepository)
        {
            _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
        }

        public async ValueTask<(Guid TransactionId, decimal PostBalance)> ExchangeAsync(
            Guid brandId,
            Guid walletId,
            string playerCode,
            TransactionTypes transactionType,
            decimal amount,
            string? description,
            string? externalId,
            Guid? createdByUserId)
        {
            const string sql = "dbo.wallet_update_wallet_balance";
            var updatedOn = DateTimeOffset.UtcNow;
            var transactionId = GuidExtensions.NewSequentialGuid();

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var postBalance = await _tgsRepository.SqlQuerySingleAsync<decimal>(
                sql,
                parameters: new Dictionary<string, object>
                {
                    {":player_code", playerCode},
                    {":wallet_id", walletId},
                    {":amount", amount},
                    {":updated_on", updatedOn}
                }).ConfigureAwait(false);

            await _tgsRepository.Transactions
                .AddAsync(new()
                {
                    Id = transactionId,
                    PlayerCode = playerCode,
                    WalletId = walletId,
                    BrandId = brandId,
                    Amount = amount,
                    Balance = postBalance,
                    Type = transactionType switch
                    {
                        TransactionTypes.Deposit                => TransactionType.Deposit,
                        TransactionTypes.Withdrawal             => TransactionType.Withdrawal,
                        TransactionTypes.Adjust                 => TransactionType.Adjust,
                        TransactionTypes.Bonus                  => TransactionType.Bonus,
                        TransactionTypes.Rebate                 => TransactionType.Rebate,
                        TransactionTypes.Dividend               => TransactionType.Dividend,
                        TransactionTypes.Promotion              => TransactionType.Promotion,
                        TransactionTypes.GameProviderFundIn     => TransactionType.GameProviderFundIn,
                        TransactionTypes.GameProviderFundOut    => TransactionType.GameProviderFundOut,
                        TransactionTypes.CancelWithdrawal       => TransactionType.CancelWithdrawal,
                        TransactionTypes.GameProviderDeposit    => TransactionType.GameProviderDeposit,
                        TransactionTypes.GameProviderWithdrawal => TransactionType.GameProviderWithdrawal,
                        _                                       => TransactionType.Other
                    },
                    Description = description,
                    RecordedOn = DateTimeOffset.Now,
                    ExternalTransactionId = externalId,
                    CreatedByUserId = createdByUserId
                })
                .ConfigureAwait(false);

            await _tgsRepository.SaveChangesAsync().ConfigureAwait(false);

            scope.Complete();

            return (transactionId, postBalance);
        }

        public async ValueTask<decimal> GetBalanceAsync(Guid walletId, string playerCode)
        {
            var playerWallet = await _tgsRepository.Wallets
                .FindAsync(walletId)
                .ConfigureAwait(false);

            if (playerWallet.PlayerCode == playerCode)
                return playerWallet.Balance;

            throw new Exception("Can't find player wallet.");
        }
    }
}