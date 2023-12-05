using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.CashService.Client;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Wallets;

internal class GetPlayerBalanceQueryService(
    IPlayerServiceClient playerServiceClient,
    ICashServiceClient cashServiceClient)
    : IGetService<GetPlayerBalanceQuery, decimal>
{
    private readonly ICashServiceClient _cashServiceClient = cashServiceClient ?? throw new ArgumentNullException(nameof(cashServiceClient));
    private readonly IPlayerServiceClient _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));

    public async ValueTask<decimal> GetAsync(GetPlayerBalanceQuery query, CancellationToken cancellationToken = default)
    {
        var playerProps = await _playerServiceClient.GetPlayerPropertiesAsync(
            query.PlayerCode,
            cancellationToken).ConfigureAwait(false);

        if (playerProps is null)
            return 0M;

        var walletId = Guid.Parse(playerProps["DefaultWalletId"]);

        return await _cashServiceClient.GetBalanceAsync(
            walletId,
            cancellationToken).ConfigureAwait(false);
    }
}
