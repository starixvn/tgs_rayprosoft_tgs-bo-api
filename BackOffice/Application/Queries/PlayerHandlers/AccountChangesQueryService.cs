using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.CashService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.PlayerHandlers;

internal class AccountChangesQueryService : IGetService<AccountChangesQuery, PagingResponse<TransactionLog>>
{
    private readonly ICashServiceClient _cashServiceClient;

    public AccountChangesQueryService(ICashServiceClient cashServiceClient)
    {
        _cashServiceClient = cashServiceClient ?? throw new ArgumentNullException(nameof(cashServiceClient));
    }

    public ValueTask<PagingResponse<TransactionLog>> GetAsync(AccountChangesQuery query, CancellationToken cancellationToken = default)
    {
        return _cashServiceClient.QueryTransactionLogsAsync(
            query.WalletId,
            query.From,
            query.To,
            query.QueryTransactionTypes,
            query.Paging,
            cancellationToken);
    }
}
