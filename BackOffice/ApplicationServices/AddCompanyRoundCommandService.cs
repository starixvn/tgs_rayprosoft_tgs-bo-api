using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate.Clients;
using Tgs.GameProviders.Pluggable.Client;
using Tgs.GameProviders.Pluggable.ViewModels;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class AddCompanyRoundCommandService(
    IPlayerServiceClient playerServiceClient,
    IGameProvidersBackofficePluggableClient pluggableClient,
    IBrandServiceClient brandServiceClient) : ICommandService<AddCompanyRoundCommand>
{
    private readonly IBrandServiceClient _brandServiceClient = brandServiceClient ?? throw new ArgumentNullException(nameof(brandServiceClient));
    private readonly IPlayerServiceClient _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
    private readonly IGameProvidersBackofficePluggableClient _pluggableClient = pluggableClient ?? throw new ArgumentNullException(nameof(pluggableClient));

    public async ValueTask ExecuteAsync(AddCompanyRoundCommand command, CancellationToken cancellationToken = default)
    {
        var playerInfo = await _playerServiceClient
            .GetPlayerInfoAsync(command.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        var playerProps = await _playerServiceClient
            .GetPlayerPropertiesAsync(command.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        var playerWalletId = Guid.Parse(playerProps["DefaultWalletId"]);

        var brand = await _brandServiceClient
            .GetBrandInfoAsync(playerInfo.BrandId, cancellationToken)
            .ConfigureAwait(false);

        await _pluggableClient.AddCompanyRoundAsync(new AddCompanyRoundViewModel
        {
            PlayerCode = command.PlayerCode,
            WalletId = playerWalletId,
            BrandId = playerInfo.BrandId,
            CurrencyCode = brand.Currency,
            ExternalRoundId = command.ExternalRoundId,
            GameProviderId = command.GameProviderId,
            GameId = command.GameId,
            ExternalCreatedOn = command.ExternalCreatedOn,
            ExternalClosedOn = command.ExternalClosedOn,
            BetAmount = command.BetAmount,
            ValidBetAmount = command.ValidBetAmount,
            SettleAmount = command.SettleAmount,
            Commission = command.Commission,
            Turnover = command.Turnover,
            CompanyWinLoss = command.CompanyWinLoss,
            ClientRoundStatus = command.ClientRoundStatus,
            OperatorId = Guid.Parse(command.OperatorId),
            Remark = command.Remark,
        }, cancellationToken);
    }
}
