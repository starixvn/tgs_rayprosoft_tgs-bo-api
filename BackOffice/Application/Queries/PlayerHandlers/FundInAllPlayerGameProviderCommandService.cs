using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.GameProviders.Pluggable.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.PlayerHandlers;

internal class FundInAllPlayerGameProviderCommandService(
    ILogger<FundInAllPlayerGameProviderCommandService> logger,
    IPlayerService playerService,
    IGameProvidersBackofficePluggableClient gameProvidersBackofficePluggableClient,
    IBrandGameProviderService brandGameProviderService) : ICommandService<FundInAllPlayerGameProviderCommand>
{
    private readonly IBrandGameProviderService _brandGameProviderService = brandGameProviderService ??
            throw new ArgumentNullException(nameof(brandGameProviderService));
    private readonly IGameProvidersBackofficePluggableClient _gameProvidersBackofficePluggableClient = gameProvidersBackofficePluggableClient ??
            throw new ArgumentNullException(nameof(gameProvidersBackofficePluggableClient));
    private readonly ILogger<FundInAllPlayerGameProviderCommandService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IPlayerService _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));

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
                         try
                         {
                             await _gameProvidersBackofficePluggableClient.FundInAsync(
                                 player.PlayerCode,
                                 brandGameProvider.GameProviderCode).ConfigureAwait(false);
                         }
                         catch (Exception ex)
                         {
                             _logger.LogError(ex, "This player ({playerCode}) failed to transfer from the game provider ({gameProviderCode}) to the main wallet.",
                                 player.PlayerCode,
                                brandGameProvider.GameProviderCode);
                         }
                     }, cancellationToken: cancellationToken)).ToList();

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}
