using System;
using System.Threading.Tasks;

namespace TGS.Services.WalletService
{
    public interface IWalletRepository
    {
        ValueTask<(Guid TransactionId, decimal PostBalance)> ExchangeAsync(
            Guid brandId,
            Guid walletId,
            string playerCode,
            TransactionTypes transactionType,
            decimal amount,
            string? description = null,
            string? externalId = null,
            Guid? createdByUserId = null);

        ValueTask<decimal> GetBalanceAsync(Guid walletId, string playerCode);
    }
}