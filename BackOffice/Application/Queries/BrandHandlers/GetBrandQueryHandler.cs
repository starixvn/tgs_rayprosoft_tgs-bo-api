using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.BrandHandlers;

public class GetBrandQueryHandler : Handler<GetBrandRequest, GetBrandResponse>
{
    private readonly IBrandBetLimitService _brandBetLimitService;
    private readonly IBrandCurrencyService _brandCurrencyService;
    private readonly IBrandGameProviderService _brandGameProviderService;
    private readonly IBrandGameService _brandGameService;
    private readonly IBrandGroupService _brandGroupService;
    private readonly IBrandLanguageService _brandLanguageService;
    private readonly IBrandService _service;

    public GetBrandQueryHandler(IBrandService service, IBrandGameProviderService brandGameProviderService,
        IBrandCurrencyService brandCurrencyService,
        IBrandLanguageService brandLanguageService,
        IBrandGameService brandGameService,
        IBrandBetLimitService brandBetLimitService,
        IBrandGroupService brandGroupService,
        IHttpContextAccessor accessor) : base(accessor)
    {
        _service = service;
        _brandGameProviderService = brandGameProviderService;
        _brandCurrencyService = brandCurrencyService;
        _brandLanguageService = brandLanguageService;
        _brandGameService = brandGameService;
        _brandBetLimitService = brandBetLimitService;
        _brandGroupService = brandGroupService;
    }

    public override async Task<GetBrandResponse> Handle(GetBrandRequest request,
        CancellationToken cancellationToken)
    {
        var brand = await _service.GetBrandAsync(x => x.Id == request.Id,
            x => x
                .Where(ResellerId, r => r.Licensee.ResellerId == ResellerId)
                .Where(LicenseeId, l => l.LicenseeId == LicenseeId)
                .Where(BrandId, l => l.Id == BrandId));

        return brand == null
            ? CreateInvalidResponse<GetBrandResponse>(Brand.NotFound)
            : (new()
            {
                Valid = true,
                Brand = new()
                {
                    Id = brand.Id,
                    LicenseeId = brand.LicenseeId,
                    LicenseeName = brand.Licensee.Name,
                    BusinessType = brand.Licensee.Reseller.BusinessType,
                    Code = brand.Code,
                    Name = brand.Name,
                    Status = brand.Status,
                    AgentDividendType = brand.AgentDividendType,
                    ClientId = brand.ClientId,
                    SecretKey = brand.SecretKey,
                    ApiKey = brand.ApiKey,
                    SettlementDay = brand.SettlementDay,
                    SettlementIntervalType = brand.SettlementIntervalType,
                    PlatformCharge = brand.PlatformCharge,
                    CheckDividendActiveCount = brand.CheckDividendActiveCount,
                    Domains = brand.Domains.Select(x => x.Url).ToArray(),
                    Currencies = await _brandCurrencyService.GetBrandCurrenciesQueries()
                    .Where(x => x.BrandId == request.Id)
                    .Select(x => new BrandCurrencyModel
                    {
                        Code = x.CurrencyCode,
                        Name = x.Currency.Name
                    }).ToArrayAsync(cancellationToken),
                    Languages = await _brandLanguageService.GetBrandLanguagesQueries()
                    .Where(x => x.BrandId == request.Id)
                    .Select(x => new BrandLanguageModel
                    {
                        CultureCode = x.CultureCode,
                        BrandCultureCode = x.BrandCultureCode
                    }).ToArrayAsync(cancellationToken),
                    GameProviders = await _brandGameProviderService.GetBrandGameProviderQueries()
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
                    await _brandGameService.GetBrandGameQueries().Where(x => x.BrandId == request.Id).Select(x
                        => new BrandGameModel
                        {
                            GameProviderId = x.Game.GameProviderId,
                            GameId = x.GameId,
                            GameName = x.Game.Name,
                            GameType = x.Game.Type
                        }).ToArrayAsync(cancellationToken),
                    BetLimits =
                    await _brandBetLimitService.GetBrandBetLimitsQueries().Where(x => x.BrandId == request.Id)
                        .Select(x => new BrandBetLimitModel
                        {
                            Id = x.BetLimitId,
                            CurrencyCode = x.BetLimit.CurrencyCode,
                            IsDefault = x.IsDefault
                        }).ToArrayAsync(cancellationToken),
                    BrandGroups = await _brandGroupService.GetBrandGroupsQueries().Where(x => x.BrandId == request.Id)
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
                    TimeSpanMinutes = (int)brand.TimeSpanMinutes!
                }
            });
    }
}
