using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tgs.GameProviders.Pluggable.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

internal class GetPlayerGameProviderBalanceQueryService(
    IPlayerService playerService,
    IGameProvidersBackofficePluggableClient gameProvidersBackofficePluggableClient,
    IBrandGameProviderService brandGameProviderService) : IQueryService<GetPlayerGameProviderBalanceQuery, GetPlayerGameProviderBalanceResponse>
{
    private readonly IBrandGameProviderService _brandGameProviderService = brandGameProviderService ??
            throw new ArgumentNullException(nameof(brandGameProviderService));
    private readonly IGameProvidersBackofficePluggableClient _gameProvidersBackofficePluggableClient = gameProvidersBackofficePluggableClient ??
            throw new ArgumentNullException(nameof(gameProvidersBackofficePluggableClient));
    private readonly IPlayerService _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));

    public async IAsyncEnumerable<GetPlayerGameProviderBalanceResponse> QueryAsync(
        GetPlayerGameProviderBalanceQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var gameProviderBalances = new List<GetPlayerGameProviderBalanceResponse>();

        var player = await _playerService
            .GetPlayerAsync(
            p => p.PlayerCode == query.PlayerCode)
            .ConfigureAwait(false);

        var brandGameProviders = await _brandGameProviderService
            .GetBrandGameProviderQueries(preQuery => preQuery.Where(
                gp => gp.BrandId == player.BrandId
                    && gp.GameProvider.TransferWallet
                    && gp.GameProvider.Currencies.Any(
                        currency => currency.CurrencyCode.Contains(player.DefaultWalletCurrencyCode))))
            .Select(x => new
            {
                GameProviderCode = x.GameProvider.Code,
                GameProviderName = x.GameProvider.Name,
                GameProviderCurrencies = x.GameProvider.Currencies
            })
            .OrderBy(x => x.GameProviderCode)
            .ToArrayAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var tasks = new List<Task>();

        foreach (var brandGameProvider in brandGameProviders)
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var playerBalanceInfo = await _gameProvidersBackofficePluggableClient
                        .GetBalanceInfoAsync(
                        playerCode: player.PlayerCode,
                        gameProviderCode: brandGameProvider.GameProviderCode,
                        cancellationToken: cancellationToken)
                        .AsTask()
                        .WaitAsync(
                        timeout: TimeSpan.FromSeconds(15),
                        cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    gameProviderBalances.Add(new GetPlayerGameProviderBalanceResponse(
                        GameProviderCode: brandGameProvider.GameProviderCode,
                        GameProviderName: brandGameProvider.GameProviderName,
                        IsMainWallet: false,
                        IsMaintain: false,
                        Balance: playerBalanceInfo.Balance,
                        FreeBalance: playerBalanceInfo.FreeBalance,
                        OutstandingBalance: playerBalanceInfo.OutstandingBalance));
                }
                catch
                {
                    gameProviderBalances.Add(new GetPlayerGameProviderBalanceResponse(
                        GameProviderCode: brandGameProvider.GameProviderCode,
                        GameProviderName: brandGameProvider.GameProviderName,
                        IsMainWallet: false,
                        IsMaintain: true,
                        Balance: null,
                        FreeBalance: null,
                        OutstandingBalance: null));
                }
            }, cancellationToken: cancellationToken));
        }

        await Task.WhenAll(tasks);

        if (gameProviderBalances.Count == 0)
            yield break;

        foreach (var gameProviderBalance in gameProviderBalances)
        {
            yield return gameProviderBalance;
        }
    }
}
