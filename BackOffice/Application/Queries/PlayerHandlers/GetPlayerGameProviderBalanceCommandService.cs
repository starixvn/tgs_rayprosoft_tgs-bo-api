using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Tgs.CashService.Client;
using Tgs.GameProviders.Pluggable.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.PlayerHandlers;

internal class GetPlayerGameProviderBalanceCommandService : IQueryService<GetPlayerGameProviderBalanceCommand, GetPlayerGameProviderBalanceResponse>
{
    private readonly IBrandGameProviderService _brandGameProviderService;
    private readonly ICashServiceClient _cashServiceClient;
    private readonly IGameProvidersBackofficePluggableClient _gameProvidersBackofficePluggableClient;
    private readonly IPlayerService _playerService;

    public GetPlayerGameProviderBalanceCommandService(
        IPlayerService playerService,
        ICashServiceClient cashServiceClient,
        IGameProvidersBackofficePluggableClient gameProvidersBackofficePluggableClient,
        IBrandGameProviderService brandGameProviderService)
    {
        _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
        _cashServiceClient = cashServiceClient ?? throw new ArgumentNullException(nameof(cashServiceClient));
        _gameProvidersBackofficePluggableClient = gameProvidersBackofficePluggableClient ??
            throw new ArgumentNullException(nameof(gameProvidersBackofficePluggableClient));
        _brandGameProviderService = brandGameProviderService ??
            throw new ArgumentNullException(nameof(brandGameProviderService));
    }

    public async IAsyncEnumerable<GetPlayerGameProviderBalanceResponse> QueryAsync(
        GetPlayerGameProviderBalanceCommand query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var gameProviderBalances = new List<GetPlayerGameProviderBalanceResponse>();
        var player = await _playerService.GetPlayerAsync(p => p.PlayerCode == query.PlayerCode);
        var brandGameProviders = _brandGameProviderService
            .GetBrandGameProviderQueries(q => q.Where(x =>
                x.BrandId == player.BrandId &&
                x.GameProvider.TransferWallet &&
                x.GameProvider.Currencies.Any(currency
                    => currency.CurrencyCode.Contains(player.DefaultWalletCurrencyCode))))
            .Select(x => new
            {
                GameProviderCode = x.GameProvider.Code,
                GameProviderName = x.GameProvider.Name,
                GameProviderCurrencies = x.GameProvider.Currencies
            })
            .OrderBy(x => x.GameProviderCode)
            .ToArray();

        try
        {
            var walletBalance = await _cashServiceClient.GetBalanceAsync(
                player.DefaultWalletId,
                cancellationToken).ConfigureAwait(false);

            gameProviderBalances.Add(new GetPlayerGameProviderBalanceResponse(
                GameProviderCode: "DEFAULT",
                GameProviderName: "DEFAULT",
                IsMainWallet: true,
                IsMaintain: false,
                Balance: walletBalance));
        }
        catch
        {
            gameProviderBalances.Add(new GetPlayerGameProviderBalanceResponse(
                GameProviderCode: "DEFAULT",
                GameProviderName: "DEFAULT",
                IsMainWallet: true,
                IsMaintain: true,
                Balance: null));
        }

        var tasks = new List<Task>();

        foreach (var brandGameProvider in brandGameProviders)
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var balance = default(decimal?);

                    balance = await _gameProvidersBackofficePluggableClient
                        .GetBalanceAsync(
                            player.PlayerCode,
                            brandGameProvider.GameProviderCode)
                        .ConfigureAwait(false);

                    gameProviderBalances.Add(new GetPlayerGameProviderBalanceResponse(
                        GameProviderCode: brandGameProvider.GameProviderCode,
                        GameProviderName: brandGameProvider.GameProviderName,
                        IsMainWallet: false,
                        IsMaintain: false,
                        Balance: balance));
                }
                catch
                {
                    gameProviderBalances.Add(new GetPlayerGameProviderBalanceResponse(
                        GameProviderCode: brandGameProvider.GameProviderCode,
                        GameProviderName: brandGameProvider.GameProviderName,
                        IsMainWallet: false,
                        IsMaintain: true,
                        Balance: null));
                }
            }, cancellationToken: cancellationToken));
        }

        await Task.WhenAll(tasks);

        if (!gameProviderBalances.Any())
        {
            yield break;
        }

        foreach (var gameProviderBalance in gameProviderBalances)
        {
            yield return gameProviderBalance;
        }
    }
}
