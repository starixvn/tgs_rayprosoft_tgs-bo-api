using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.CashService.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.PlayerHandlers;

internal class GetPlayerMainWalletBalanceHandler : IGetService<GetPlayerMainWalletBalance, decimal>
{
    private readonly ICashServiceClient _cashServiceClient;
    private readonly IPlayerService _playerService;

    public GetPlayerMainWalletBalanceHandler(
        ICashServiceClient cashServiceClient,
        IPlayerService playerService)
    {
        _cashServiceClient = cashServiceClient ?? throw new ArgumentNullException(nameof(cashServiceClient));
        _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
    }

    public async ValueTask<decimal> GetAsync(GetPlayerMainWalletBalance query, CancellationToken cancellationToken = default)
    {
        var player = await _playerService.GetPlayerAsync(x => x.PlayerCode == query.PlayerCode).ConfigureAwait(false);

        return await _cashServiceClient.GetBalanceAsync(
            player.DefaultWalletId,
            cancellationToken).ConfigureAwait(false);
    }
}
