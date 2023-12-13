using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.CashService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Wallets;

internal class AccountChangesQueryService(
    ICashServiceClient cashServiceClient)
    : IGetService<AccountChangesQuery, PagingResponse<TransactionLog>>
{
    public async ValueTask<PagingResponse<TransactionLog>> GetAsync(AccountChangesQuery query, CancellationToken cancellationToken = default)
    {
        var queryTransactionTypes = Array.Empty<int>();

        queryTransactionTypes = query.AccountChangeType is null
            ? Enum.GetValues(typeof(Domain.Enums.TransactionType))
                .Cast<Domain.Enums.TransactionType>()
                .Where(x => x is not Domain.Enums.TransactionType.WithdrawCommit
                    and not Domain.Enums.TransactionType.TransferCommit
                    and not Domain.Enums.TransactionType.Receive)
                .Select(x => (int)x)
                .ToArray()
            : query.AccountChangeType.Value switch
            {
                Domain.Enums.TransactionType.Deposit => [
                    (int)Domain.Enums.TransactionType.Deposit,
                    (int)Domain.Enums.TransactionType.Deposited
                ],
                Domain.Enums.TransactionType.Withdrawal => [
                    (int)Domain.Enums.TransactionType.Withdrawal,
                    (int)Domain.Enums.TransactionType.WithdrawLock
                ],
                Domain.Enums.TransactionType.CancelWithdrawal => [
                    (int)Domain.Enums.TransactionType.CancelWithdrawal,
                    (int)Domain.Enums.TransactionType.WithdrawRollback
                ],
                _ => [
                    (int)query.AccountChangeType.Value
                ],
            };

        return await cashServiceClient.QueryTransactionLogsAsync(
            query.WalletId,
            query.From,
            query.To,
            queryTransactionTypes,
            query.Paging,
            cancellationToken);
    }
}
