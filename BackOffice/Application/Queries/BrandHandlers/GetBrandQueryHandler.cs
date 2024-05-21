using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Tgs.BrandCertificate.Clients;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.BrandHandlers;

public class GetBrandQueryHandler(
    IBrandGameProviderService brandGameProviderService,
    IBrandCurrencyService brandCurrencyService,
    IBrandLanguageService brandLanguageService,
    IBrandGameService brandGameService,
    IBrandBetLimitService brandBetLimitService,
    IBrandGroupService brandGroupService,
    IBrandServiceClient brandServiceClient,
    IHttpContextAccessor accessor) : Handler<GetBrandRequest, GetBrandResponse>(accessor)
{
    public override async Task<GetBrandResponse> Handle(GetBrandRequest request,
        CancellationToken cancellationToken)
    {
        var brand = await brandServiceClient.GetBrandFullInfoAsync(request.Id, cancellationToken);

        if (brand is null ||
            ResellerId != null && brand.ResellerId != ResellerId ||
            LicenseeId != null && brand.LicenseeId != LicenseeId)
        {
            return CreateInvalidResponse<GetBrandResponse>(Brand.NotFound);
        }

        var brandDomains = await brandServiceClient.GetBrandDomainsAsync(request.Id, cancellationToken)
            .ToArrayAsync(cancellationToken);

        return new GetBrandResponse
        {
            Valid = true,
            Brand = new BrandModel
            {
                Id = brand.Id,
                LicenseeId = brand.LicenseeId,
                LicenseeName = brand.LicenseeName,
                Code = brand.Code,
                Name = brand.Name,
                Status = (BrandStatus)brand.Status,
                AgentDividendType = (AgentDividendType)brand.AgentDividendType,
                ClientId = brand.ClientId,
                SecretKey = brand.SecretKey,
                ApiKey = brand.ApiKey,
                SettlementDay = brand.SettlementDay,
                SettlementIntervalType = (SettlementIntervalType)brand.SettlementIntervalType,
                PlatformCharge = brand.PlatformCharge,
                CheckDividendActiveCount = brand.CheckDividendActiveCount,
                Domains = brandDomains,
                Currencies = await brandCurrencyService.GetBrandCurrenciesQueries()
                    .Where(x => x.BrandId == request.Id)
                    .Select(x => new BrandCurrencyModel
                    {
                        Code = x.CurrencyCode,
                        Name = x.Currency.Name
                    }).ToArrayAsync(cancellationToken),
                Languages = await brandLanguageService.GetBrandLanguagesQueries()
                    .Where(x => x.BrandId == request.Id)
                    .Select(x => new BrandLanguageModel
                    {
                        CultureCode = x.CultureCode,
                        BrandCultureCode = x.BrandCultureCode
                    }).ToArrayAsync(cancellationToken),
                GameProviders = await brandGameProviderService.GetBrandGameProviderQueries()
                    .Where(x => x.BrandId == request.Id)
                    .Select(x => new BrandGameProviderModel
                    {
                        Id = x.Id,
                        GameProviderId = x.GameProviderId,
                        Name = x.GameProvider.Name,
                        ConfigurationId = x.GameProviderConfigurationId,
                        DefaultLanguageCode = x.DefaultLanguageCode,
                        DesktopQueryString = x.DesktopQueryString,
                        MobileQueryString = x.MobileQueryString
                    }).ToArrayAsync(cancellationToken),
                Games =
                    await brandGameService.GetBrandGameQueries().Where(x => x.BrandId == request.Id).Select(x
                        => new BrandGameModel
                        {
                            GameProviderId = x.Game.GameProviderId,
                            GameId = x.GameId,
                            GameName = x.Game.Name,
                            GameType = x.Game.Type
                        }).ToArrayAsync(cancellationToken),
                BetLimits =
                    await brandBetLimitService.GetBrandBetLimitsQueries().Where(x => x.BrandId == request.Id)
                        .Select(x => new BrandBetLimitModel
                        {
                            Id = x.BetLimitId,
                            CurrencyCode = x.BetLimit.CurrencyCode,
                            IsDefault = x.IsDefault
                        }).ToArrayAsync(cancellationToken),
                BrandGroups = await brandGroupService.GetBrandGroupsQueries().Where(x => x.BrandId == request.Id)
                    .Select(x => new BrandGroupModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        IsSports = x.IsSports,
                        BrandGames = x.BrandGames.Select(g => new BrandGameModel
                        {
                            GameProviderId = g.BrandGame.Game.GameProviderId,
                            GameId = g.BrandGame.GameId,
                            GameName = g.BrandGame.Game.Name,
                            GameType = g.BrandGame.Game.Type
                        }).ToArray(),
                        GameProviderBetLimits = x.BrandGroupBetLimits.SelectMany(bgb => bgb.GameProviderBetLimits)
                            .Select(b => new BrandGroupGameProviderBetLimitModel
                            {
                                GameProviderId = b.GameProviderBetLimit.GameProviderId,
                                BrandBetLimitId = b.BrandGroupBetLimit.BetLimitId,
                                GameProviderBetLimitId = b.GameProviderBetLimitId
                            }).ToArray()
                    }).ToArrayAsync(cancellationToken),
                Description = brand.Description,
                OperationalEmail = brand.OperationalEmail,
                CreatedOn = brand.CreatedOn,
                UpdatedOn = brand.UpdatedOn,
                EnablePlayerIdentity = brand.EnablePlayerIdentity,
                AutoClearWageringRequirement = brand.AutoClearWageringRequirement,
                AutoClearWageringRequirementBalance = brand.AutoClearWageringRequirementBalance,
                DepositHandlingFeeRateTenPercentile = brand.DepositHandlingFeeRateTenPercentile,
                WithdrawHandlingFeeRateTenPercentile = brand.WithdrawHandlingFeeRateTenPercentile,
                TimeSpanMinutes = (int)brand.TimeSpanMinutes!,
                EnablePlayerAgent = brand.EnablePlayerAgent,
                AutoSendAgentBonus = brand.AutoSendAgentBonus,
                EnableSms = brand.SmsEnabled,
                SmsVendorCode = brand.SmsEnabled
                    ? (await brandServiceClient
                        .GetBrandSmsSettingsAsync<JsonDocument>(request.Id, cancellationToken: cancellationToken)
                        .ConfigureAwait(false)).Code
                    : null
            }
        };
    }
}