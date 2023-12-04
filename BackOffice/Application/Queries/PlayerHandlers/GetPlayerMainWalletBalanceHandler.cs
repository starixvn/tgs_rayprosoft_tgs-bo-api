using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.CashService.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.PlayerHandlers;

internal class GetPlayerMainWalletBalanceHandler(
    ICashServiceClient cashServiceClient,
    IPlayerService playerService) : IGetService<GetPlayerMainWalletBalance, decimal>
{
    private readonly ICashServiceClient _cashServiceClient = cashServiceClient ?? throw new ArgumentNullException(nameof(cashServiceClient));
    private readonly IPlayerService _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));

    public async ValueTask<decimal> GetAsync(GetPlayerMainWalletBalance query, CancellationToken cancellationToken = default)
    {
        var player = await _playerService.GetPlayerAsync(x => x.PlayerCode == query.PlayerCode).ConfigureAwait(false);

        return await _cashServiceClient.GetBalanceAsync(
            player.DefaultWalletId,
            cancellationToken).ConfigureAwait(false);
    }
}
