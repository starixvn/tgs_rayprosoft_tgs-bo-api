using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.CashService.Client;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.PlayerHandlers;

internal class AccountChangesQueryService : IGetService<AccountChangesQuery, PagingResponse<TransactionLog>>
{
    private readonly ICashServiceClient _cashServiceClient;
    private readonly IPlayerServiceClient _playerServiceClient;

    public AccountChangesQueryService(
        IPlayerServiceClient playerServiceClient,
        ICashServiceClient cashServiceClient)
    {
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
        _cashServiceClient = cashServiceClient ?? throw new ArgumentNullException(nameof(cashServiceClient));
    }

    public async ValueTask<PagingResponse<TransactionLog>> GetAsync(AccountChangesQuery query, CancellationToken cancellationToken = default)
    {
        var playerProps = await _playerServiceClient
            .GetPlayerPropertiesAsync(query.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        var playerWalletId = Guid.Parse(playerProps["DefaultWalletId"]!);

        var queryTransactionTypes = Array.Empty<int>();

        queryTransactionTypes = query.AccountChangeType is null
            ? Enum
                .GetValues(typeof(Domain.Enums.TransactionType))
                .Cast<Domain.Enums.TransactionType>()
                .Where(x => x is not Domain.Enums.TransactionType.WithdrawCommit
                    and not Domain.Enums.TransactionType.TransferCommit)
                .Select(x => (int)x)
                .ToArray()
            : query.AccountChangeType.Value switch
            {
                Domain.Enums.TransactionType.Deposit => new int[] {
                    (int)Domain.Enums.TransactionType.Deposit,
                    (int)Domain.Enums.TransactionType.Deposited
                },
                Domain.Enums.TransactionType.Withdrawal => new int[] {
                    (int)Domain.Enums.TransactionType.Withdrawal,
                    (int)Domain.Enums.TransactionType.WithdrawLock
                },
                Domain.Enums.TransactionType.CancelWithdrawal => new int[] {
                    (int)Domain.Enums.TransactionType.CancelWithdrawal,
                    (int)Domain.Enums.TransactionType.WithdrawRollback
                },
                _ => new int[] {
                    (int)query.AccountChangeType.Value
                },
            };

        return await _cashServiceClient.QueryTransactionLogsAsync(
            playerWalletId,
            query.From,
            query.To,
            queryTransactionTypes,
            query.Paging,
            cancellationToken);
    }
}
