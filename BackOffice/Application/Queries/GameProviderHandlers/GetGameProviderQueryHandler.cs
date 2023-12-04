using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Services.BackOffice.Application.Models.GameProvider;
using TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.GameProviderHandlers;

public class GetGameProviderQueryHandler(IGameProviderService service, IHttpContextAccessor accessor) : Handler<GetGameProviderRequest, GetGameProviderResponse>(accessor)
{
    private readonly IGameProviderService _service = service;

    public override async Task<GetGameProviderResponse> Handle(GetGameProviderRequest request,
        CancellationToken cancellationToken)
    {
        var gameProvider = await _service.GetGameProviderAsync(x => x.Id == request.Id, x => x
            .Where(ResellerId, r => r.Resellers.Select(x => x.ResellerId).Contains(ResellerId.Value))
            .Where(LicenseeId, l => l.Licensees.Select(x => x.LicenseeId).Contains(LicenseeId.Value))
            .Where(BrandId, b => b.Brands.Select(x => x.BrandId).Contains(BrandId.Value))
        );

        return gameProvider == null
            ? CreateInvalidResponse<GetGameProviderResponse>(GameProvider.NotFound)
            : new()
            {
                Valid = true,
                GameProvider = new()
                {
                    Id = gameProvider.Id,
                    Name = gameProvider.Name,
                    Code = gameProvider.Code,
                    UniqueId = gameProvider.UniqueId,
                    Description = gameProvider.Description,
                    AutoCloseRound = gameProvider.AutoCloseRound,
                    SupportTestPlayer = gameProvider.SupportTestPlayer,
                    AllowKickPlayer = gameProvider.AllowKickPlayer,
                    PlaceBetTimeout = gameProvider.PlaceBetTimeout,
                    SettleBetTimeout = gameProvider.SettleBetTimeout,
                    IsTransferWallet = gameProvider.TransferWallet,
                    Currencies =
                    gameProvider.Currencies.Select(x => new GameProviderCurrencyModel
                    {
                        Code = x.CurrencyCode,
                        GameProviderCurrencyCode = x.GameProviderCurrencyCode
                    }).ToArray(),
                    Languages =
                    gameProvider.Languages.Select(x => new GameProviderLanguageModel
                    {
                        CultureCode = x.CultureCode,
                        GameProviderCultureCode = x.GameProviderCultureCode
                    }).ToArray(),
                    BetLimits = gameProvider.BetLimits.Select(x => new GameProviderBetLimitModel
                    {
                        Id = x.Id,
                        CurrencyCode = x.CurrencyCode,
                        MinBet = x.MinBet,
                        MaxBet = x.MaxBet,
                        MaxBetPerMatch = x.MaxBetPerMatch,
                        ExternalSetting = x.ExternalSetting
                    }).ToArray(),
                    UpdatedByUsername = gameProvider.UpdatedByUsername,
                    UpdatedOn = gameProvider.UpdatedOn
                }
            };
    }
}
