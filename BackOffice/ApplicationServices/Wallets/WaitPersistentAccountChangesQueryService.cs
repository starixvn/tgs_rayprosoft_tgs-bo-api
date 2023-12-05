using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.CashService.Client;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Wallets;

internal class WaitPersistentAccountChangesQueryService(
    IPlayerServiceClient playerServiceClient,
    ICashServiceClient cashServiceClient)
    : IGetService<WaitPersistentAccountChangesQuery, PagingResponse<TransactionLog>>
{
    public async ValueTask<PagingResponse<TransactionLog>> GetAsync(WaitPersistentAccountChangesQuery query, CancellationToken cancellationToken = default)
    {
        var playerProps = await playerServiceClient
            .GetPlayerPropertiesAsync(query.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        var playerWalletId = Guid.Parse(playerProps["DefaultWalletId"]!);

        return await cashServiceClient.QueryInRedisTransactionLogsAsync(
            playerWalletId,
            query.Paging,
            cancellationToken).ConfigureAwait(false);
    }
}
