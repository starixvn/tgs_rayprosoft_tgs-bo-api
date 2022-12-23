using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.GameProviders.Pluggable.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.PlayerHandlers;

internal class FundInAllPlayerGameProviderCommandService : ICommandService<FundInAllPlayerGameProviderCommand>
{
    private readonly IBrandGameProviderService _brandGameProviderService;
    private readonly IGameProvidersBackofficePluggableClient _gameProvidersBackofficePluggableClient;
    private readonly IPlayerService _playerService;

    public FundInAllPlayerGameProviderCommandService(
        IPlayerService playerService,
        IGameProvidersBackofficePluggableClient gameProvidersBackofficePluggableClient,
        IBrandGameProviderService brandGameProviderService)
    {
        _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
        _gameProvidersBackofficePluggableClient = gameProvidersBackofficePluggableClient ??
            throw new ArgumentNullException(nameof(gameProvidersBackofficePluggableClient));
        _brandGameProviderService = brandGameProviderService ??
            throw new ArgumentNullException(nameof(brandGameProviderService));
    }

    public async ValueTask ExecuteAsync(FundInAllPlayerGameProviderCommand request, CancellationToken cancellationToken)
    {
        var player = await _playerService.GetPlayerAsync(p => p.PlayerCode == request.PlayerCode);
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

        var tasks = (from brandGameProvider in brandGameProviders
                     select Task.Run(async () =>
                     {
                         await _gameProvidersBackofficePluggableClient.FundInAsync(
                             player.PlayerCode,
                             brandGameProvider.GameProviderCode).ConfigureAwait(false);
                     }, cancellationToken: cancellationToken)).ToList();

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}
