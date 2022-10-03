using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Tgs.Core.Infrastructures.Utility;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Infrastructures.Redis;
using TGS.Core.Models;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Infrastructure.RedisServices;
using TGS.Services.BackOffice.Infrastructure.Services;
using IsolationLevel = System.Data.IsolationLevel;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IBrandService
{
    Task ChangeStatusAsync(Brand brand, UpdateBrandStatusRequest request);

    Task CreateBrandAsync(CreateBrandRequest request);

    Task<bool> ExistsAsync(Expression<Func<Brand, bool>> predicate);

    Task<Brand> GetBrandAsync(Expression<Func<Brand, bool>> predicate,
                                Func<IQueryable<Brand>, IQueryable<Brand>> preQuery = null);

    IQueryable<Brand> GetBrandQueries(Func<IQueryable<Brand>, IQueryable<Brand>> preQuery = null,
        bool tracking = false);

    Task SyncBrandGamesAsync(Guid brandId);

    Task UpdateBrandAsync(Brand brand, UpdateBrandRequest request);
}

public class BrandService : BaseRepositoryOperations<ITgsRepository>, IBrandService
{
    private readonly IRedisCacheManager _cacheManager;
    private readonly ICredentialGenerator _credentialGenerator;
    private readonly IMaintenanceRedisService _maintenanceRedisService;
    private readonly ITgsRepository _repository;

    public BrandService(
        ITgsRepository tgsRepository,
        ICredentialGenerator credentialGenerator,
        IRedisCacheManager cacheManager,
        IMaintenanceRedisService maintenanceRedisService) : base(
        tgsRepository)
    {
        _repository = tgsRepository;
        _credentialGenerator = credentialGenerator;
        _cacheManager = cacheManager;
        _maintenanceRedisService = maintenanceRedisService ??
            throw new ArgumentNullException(nameof(maintenanceRedisService));
    }

    async Task IBrandService.ChangeStatusAsync(Brand brand, UpdateBrandStatusRequest request)
    {
        brand.Status = request.Status;
        brand.UpdatedOn = DateTimeOffset.Now;
        brand.UpdatedByUsername = request.EditorUsername;

        await UpdateEntityAsync(brand);
        await _cacheManager.ClearCacheAsync(RedisCacheType.Brand, brand.Id);
        if (request.Status is BrandStatus.Active or BrandStatus.Maintenance)
        {
            var licensee = _repository.Licensees.AsQueryable().First(x => x.Id == brand.LicenseeId);
            var reseller = _repository.Resellers.AsQueryable().First(x => x.Id == licensee.ResellerId);
            var isActive = reseller.Status is Status.Active && licensee.Status is Status.Active &&
                request.Status is BrandStatus.Active;
            if (isActive)
            {
                await _maintenanceRedisService.CleartBrandMaintenanceAsync(brand.Id).ConfigureAwait(false);
            }
            else
            {
                await _maintenanceRedisService.AddBrandMaintenanceAsync(brand.Id).ConfigureAwait(false);
            }
        }
    }

    async Task IBrandService.CreateBrandAsync(CreateBrandRequest request)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var timeSpan = new TimeSpan(8, 0, 0);
        var now = new DateTimeOffset(DateTimeOffset.Now.Year, DateTimeOffset.Now.Month,
            DateTimeOffset.Now.Day, 0,
            0, 0, timeSpan);

        var (currentFromDate, currentToDate, nextFromDate, nextToDate) =
            GetInterval(request.SettlementIntervalType, now, request.SettlementDay, now);

        var brand = new Brand
        {
            Id = Guid.NewGuid(),
            LicenseeId = request.LicenseeId,
            Code = request.Code,
            Name = request.Name,
            PreviousSettlementFrom = currentFromDate,
            PreviousSettlementTo = currentToDate,
            CurrentSettlementFrom = currentFromDate,
            CurrentSettlementTo = currentToDate,
            NextSettlementFrom = nextFromDate,
            NextSettlementTo = nextToDate,
            AutoSentAgentBonus = true,
            AutoSentPlayerBonus = true,
            Status = BrandStatus.Active,
            AgentDividendType = request.AgentDividendType,
            Description = request.Description,
            OperationalEmail = request.OperationalEmail,
            SettlementDay = request.SettlementDay,
            SettlementIntervalType = request.SettlementIntervalType,
            ApiKey = _credentialGenerator.GetApiKey(),
            ClientId = request.Code,
            SecretKey = _credentialGenerator.GetSecretKey(),
            CreatedByUsername = request.EditorUsername,
            UpdatedByUsername = request.EditorUsername,
            CreatedOn = DateTimeOffset.Now,
            UpdatedOn = DateTimeOffset.Now,
            PlatformCharge = request.PlatformCharge,
            CheckDividendActiveCount = request.CheckDividendActiveCount,
            EnablePlayerIdentity = request.EnablePlayerIdentity,
            AutoClearWageringRequirement = request.AutoClearWageringRequirement,
            AutoClearWageringRequirementBalance = request.AutoClearWageringRequirementBalance,
            DepositHandlingFeeRateTenPercentile = request.DepositHandlingFeeRateTenPercentile,
            WithdrawHandlingFeeRateTenPercentile = request.WithdrawHandlingFeeRateTenPercentile
        };

        brand.Languages = await GenerateBrandLanguagesAsync(brand, request.Languages);
        brand.Currencies = GenerateBrandCurrencies(brand, request.CurrencyCodes);
        brand.GameProviders = await GenerateAddBrandGameProvidersAsync(brand, request.GameProviders);
        brand.BetLimits = await GenerateBetLimitsAsync(brand, request.BetLimits);
        brand.Games = await GenerateWithCreateGamesAsync(brand);
        brand.BrandGroups = GenerateWithCreateGroups(brand, request.EditorUsername);
        brand.Domains = GenerateBrandDomains(brand, request.Domains);

        await CreateEntityAsync(brand);

        var platformCredit = new PlatformCredit
        {
            BrandId = brand.Id,
            Credit = 0,
            Balance = 0,
            CreatedByUsername = request.EditorUsername,
            UpdatedByUsername = request.EditorUsername,
            CreatedOn = DateTimeOffset.Now,
            UpdatedOn = DateTimeOffset.Now
        };

        await CreateEntityAsync(platformCredit);

        var vipLevel = new VipLevel
        {
            BrandId = brand.Id,
            Level = 1,
            CreatedByUsername = request.EditorUsername,
            UpdatedByUsername = request.EditorUsername,
            CreatedOn = DateTimeOffset.Now,
            UpdatedOn = DateTimeOffset.Now
        };

        await CreateEntityAsync(vipLevel);

        scope.Complete();
    }

    Task<bool> IBrandService.ExistsAsync(Expression<Func<Brand, bool>> predicate) => ExistsAsync(predicate);

    Task<Brand> IBrandService.GetBrandAsync(Expression<Func<Brand, bool>> predicate,
                Func<IQueryable<Brand>, IQueryable<Brand>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);

    IQueryable<Brand> IBrandService.GetBrandQueries(Func<IQueryable<Brand>, IQueryable<Brand>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);

    async Task IBrandService.SyncBrandGamesAsync(Guid brandId)
    {
        var brand = await _repository.Brands.AsQueryable().FirstOrDefaultAsync(x => x.Id == brandId);

        var brandCurrencies = brand.Currencies.Select(bc => bc.CurrencyCode);

        var baseGames = await _repository.Games.AsQueryable()
            .Where(x => x.AvailableToBrand)
            .Where(x => brand.GameProviders.Select(g => g.GameProviderId).Contains(x.GameProviderId))
            .Where(x => x.GameProvider.Currencies.Any(c => brandCurrencies.Contains(c.CurrencyCode)))
            .ToArrayAsync();

        baseGames = baseGames.Where(x
            => x.AvailableCurrencies.IsNullOrEmpty() ||
            x.AvailableCurrencies.Split(",").Any(c => brandCurrencies.Contains(c))).ToArray();

        var baseGameIds = baseGames.Select(x => x.Id).ToArray();

        var addList = baseGameIds.Except(brand.Games.Select(x => x.GameId)).ToArray();

        var games = addList.Select(id => new BrandGame
        {
            BrandId = brand.Id,
            GameId = id,
            DisplayOrder = 99,
            Rebate = true,
            EffectiveBettingPercentage = 100M
        });

        await CreateEntitiesAsync(games);

        await _cacheManager.ClearCacheAsync(RedisCacheType.BrandGames, brand.Id, true);
    }

    async Task IBrandService.UpdateBrandAsync(Brand brand, UpdateBrandRequest request)
    {
        if (request.SettlementIntervalType != brand.SettlementIntervalType)
        {
            var timeSpan = new TimeSpan(8, 0, 0);
            var now = new DateTimeOffset(DateTimeOffset.Now.Year, DateTimeOffset.Now.Month,
                DateTimeOffset.Now.Day, 0,
                0, 0, timeSpan);

            var (currentFromDate, currentToDate, nextFromDate, nextToDate) =
                GetInterval(request.SettlementIntervalType, now, request.SettlementDay,
                    brand.CurrentSettlementFrom);

            brand.CurrentSettlementFrom = currentFromDate;
            brand.CurrentSettlementTo = currentToDate;
            brand.NextSettlementFrom = nextFromDate;
            brand.NextSettlementTo = nextToDate;
        }

        brand.Code = request.Code;
        brand.Name = request.Name;
        brand.SettlementDay = request.SettlementDay;
        brand.SettlementIntervalType = request.SettlementIntervalType;
        brand.Description = request.Description;
        brand.OperationalEmail = request.OperationalEmail;
        brand.UpdatedByUsername = request.EditorUsername;
        brand.UpdatedOn = DateTimeOffset.Now;
        brand.PlatformCharge = request.PlatformCharge;
        brand.CheckDividendActiveCount = request.CheckDividendActiveCount;
        brand.EnablePlayerIdentity = request.EnablePlayerIdentity;
        brand.AutoClearWageringRequirement = request.AutoClearWageringRequirement;
        brand.AutoClearWageringRequirementBalance = request.AutoClearWageringRequirementBalance;
        brand.DepositHandlingFeeRateTenPercentile = request.DepositHandlingFeeRateTenPercentile;
        brand.WithdrawHandlingFeeRateTenPercentile = request.WithdrawHandlingFeeRateTenPercentile;

        await using var tran = await _repository.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        await DeleteEntitiesAsync(brand.BetLimits);
        await DeleteEntitiesAsync(brand.Languages);
        await DeleteEntitiesAsync(brand.Currencies);
        await DeleteEntitiesAsync(brand.BrandGroups);
        await DeleteEntitiesAsync(brand.Domains);

        brand.Languages = await GenerateBrandLanguagesAsync(brand, request.Languages);
        brand.Currencies = GenerateBrandCurrencies(brand, request.CurrencyCodes);
        brand.BetLimits = await GenerateBetLimitsAsync(brand, request.BetLimits);
        brand.Games = await GenerateWithUpdateGamesAsync(brand, request.GameProviders);
        brand.GameProviders = GenerateBrandGameProviders(brand, request.GameProviders);
        brand.BrandGroups = GenerateWithUpdateGroups(brand, request.BrandGroups,request.EditorUsername);
        brand.Domains = GenerateBrandDomains(brand, request.Domains);

        await UpdateEntityAsync(brand);
        await CreateEntitiesAsync(await GenerateAddBrandGameProvidersAsync(brand, request.GameProviders));

        await tran.CommitAsync();

        var cacheKeys = new[]
        {
            new CacheKey
            {
                CacheType = RedisCacheType.Brand,
                Key = brand.Id
            },
            new CacheKey
            {
                CacheType = RedisCacheType.Currencies,
                Key = brand.Id
            },
            new CacheKey
            {
                CacheType = RedisCacheType.Languages,
                Key = brand.Id
            },
            new CacheKey
            {
                CacheType = RedisCacheType.BrandGameProviderConfiguration,
                Key = brand.Code
            },
            new CacheKey
            {
                CacheType = RedisCacheType.BrandGameProviders,
                Key = brand.Id
            },
            new CacheKey
            {
                CacheType = RedisCacheType.BrandGames,
                Key = brand.Id
            },
            new CacheKey
            {
                CacheType = RedisCacheType.BrandDefaultBetLimit,
                Key = brand.Id
            }
        };

        await _cacheManager.ClearCacheByBatchAsync(cacheKeys);
    }

    private static HashSet<BrandGameProvider> GenerateBrandGameProviders(
        Brand brand, BrandGameProviderModel[] brandGameProviderModels)
    {
        var gameProviderIds = brandGameProviderModels.Select(x => x.GameProviderId).ToArray();
        var brandGameProviders = brand.GameProviders.Where(x => gameProviderIds.Contains(x.GameProviderId)).Select(
            x =>
            {
                var requestBrandGameProvider =
                    brandGameProviderModels.First(m => m.GameProviderId == x.GameProviderId);

                x.DefaultLanguageCode = brand.Languages.FirstOrDefault()?.CultureCode;
                x.GameProviderConfigurationId = requestBrandGameProvider.ConfigurationId;
                x.DesktopQueryString = requestBrandGameProvider.DesktopQueryString;
                x.MobileQueryString = requestBrandGameProvider.MobileQueryString;

                return x;
            }
        );

        return new(brandGameProviders);
    }

    private static HashSet<BrandGroupGame> GenerateBrandGroupGames(Brand brand, Guid brandGroupId,
        IEnumerable<int> brandGameIds)
    {
        var brandGroupGames = brandGameIds.Select(brandGameId => new BrandGroupGame
        {
            BrandGroupId = brandGroupId,
            BrandGame = brand.Games.First(g => g.GameId == brandGameId)
        });
        return new(brandGroupGames);
    }

    private static HashSet<BrandGroupGameProviderBetLimit> GenerateGameProviderBetLimits(Brand brand,
        Guid brandGroupBetLimitId, BetLimit betLimit, bool isSports)
    {
        bool BetLimitRangeFunc(GameProviderBetLimit gb) =>
            gb.CurrencyCode == betLimit.CurrencyCode && betLimit.MinBet <= gb.MinBet &&
            betLimit.MaxBet >= gb.MaxBet;

        bool BetLimitIsSportsFunc(GameProviderBetLimit b) => betLimit.MaxBetPerMatch >= b.MaxBetPerMatch;

        var brandGroupGameProviderBetLimits = brand.GameProviders.Where(x => x.GameProvider.BetLimits
                .Any(BetLimitRangeFunc))
            .Where(isSports, x => x.GameProvider.BetLimits
                .Any(BetLimitIsSportsFunc))
            .Select(x =>
            {
                var gameProviderBetLimits = x.GameProvider.BetLimits
                    .Where(BetLimitRangeFunc)
                    .Where(isSports, BetLimitIsSportsFunc)
                    .OrderBy(b => b.MinBet).ToArray();

                return new BrandGroupGameProviderBetLimit
                {
                    BrandGroupBetLimitId = brandGroupBetLimitId,
                    GameProviderBetLimitId = gameProviderBetLimits.First().Id
                };
            });

        return new(brandGroupGameProviderBetLimits);
    }

    private static HashSet<BrandGroupGame> GenerateWithUpdateBrandGroupGames(Brand brand, BrandGroup brandGroup,
        IEnumerable<int> brandGameIds)
    {
        var brandGamePoolIds = brand.Games.Select(x => x.GameId).ToArray();
        var brandGroupGames = brandGameIds?
            .Where(brandGameId => brandGamePoolIds.Contains(brandGameId)).Select(brandGameId =>
                new BrandGroupGame
                {
                    BrandGroup = brandGroup,
                    BrandGameId = brand.Games.First(g => g.GameId == brandGameId).Id
                });
        return new(brandGroupGames ?? Array.Empty<BrandGroupGame>());
    }

    private static HashSet<BrandGroupGameProviderBetLimit> GenerateWithUpdateGameProviderBetLimits(
        BrandGroupBetLimit brandGroupBetLimit, BrandGroupGameProviderBetLimitModel[] models)
    {
        var brandGroupGameProviderBetLimits = models?.Where(x => x.BrandBetLimitId == brandGroupBetLimit.BetLimitId)
            .Select(x =>
                new BrandGroupGameProviderBetLimit
                {
                    BrandGroupBetLimit = brandGroupBetLimit,
                    GameProviderBetLimitId = x.GameProviderBetLimitId
                });

        return new(brandGroupGameProviderBetLimits ?? Array.Empty<BrandGroupGameProviderBetLimit>());
    }

    private static (DateTimeOffset, DateTimeOffset, DateTimeOffset, DateTimeOffset) GetInterval(
        SettlementIntervalType type, DateTimeOffset startDate, int? dayOfMonth, DateTimeOffset currentFromDate)
    {
        switch (type)
        {
            case SettlementIntervalType.Daily:
                return (startDate, startDate, startDate.AddDays(1), startDate.AddDays(1));

            case SettlementIntervalType.Weekly:
                return (startDate,
                    DateGenerator.GetLastDayOfWeek(startDate),
                    DateGenerator.GetFirstDayOfWeek(startDate.AddDays(7)),
                    DateGenerator.GetLastDayOfWeek(startDate.AddDays(7)));

            case SettlementIntervalType.Monthly:
                if (dayOfMonth == null)
                {
                    return (startDate,
                        DateGenerator.GetLastDayOfMonth(startDate),
                        DateGenerator.GetFirstDayOfMonth(startDate.AddMonths(1)),
                        DateGenerator.GetLastDayOfMonth(startDate.AddMonths(1)));
                }

                if (startDate.Day <= dayOfMonth)
                {
                    return (currentFromDate,
                        startDate,
                        startDate.AddDays(1),
                        DateGenerator.GetSpecifyDayOfMonth(startDate, dayOfMonth.Value!));
                }

                return (currentFromDate,
                    startDate,
                    startDate.AddDays(1),
                    DateGenerator.GetSpecifyDayOfMonth(startDate.AddMonths(1),
                        dayOfMonth.Value!));

            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private async Task<HashSet<BrandGameProvider>> GenerateAddBrandGameProvidersAsync(
        Brand brand, BrandGameProviderModel[] brandGameProviderModels)
    {
        var gameProviderIds =
            brandGameProviderModels.Where(x => x.Id == null).Select(x => x.GameProviderId).ToArray();

        var gameProviders = await _repository.GameProviders.AsQueryable()
            .Where(x => gameProviderIds.Contains(x.Id)).ToArrayAsync();
        var brandGameProviders = gameProviders.Select(x =>
            {
                var requestBrandGameProvider = brandGameProviderModels.First(m => m.GameProviderId == x.Id);
                var brandGameProviderId = GuidExtensions.NewSequentialGuid();

                var newBrandGameProvider = new BrandGameProvider
                {
                    Id = brandGameProviderId,
                    BrandId = brand.Id,
                    GameProviderId = x.Id,
                    GameProvider = x,
                    DefaultLanguageCode = brand.Languages.FirstOrDefault()?.CultureCode,
                    GameProviderConfigurationId = requestBrandGameProvider.ConfigurationId,
                    DesktopQueryString = requestBrandGameProvider.DesktopQueryString,
                    MobileQueryString = requestBrandGameProvider.MobileQueryString,
                    Schedules = new BrandGameProviderSchedule[]
                    {
                        new()
                        {
                            BrandGameProviderId = brandGameProviderId,
                            From = DateTimeOffset.Now,
                            To = DateTimeOffset.Now.AddDays(7),
                            DayOfWeek = DayOfWeek.Monday,
                            IsDefault = true,
                            Status = SwitchStatus.Disabled
                        }
                    }.ToHashSet()
                };

                return newBrandGameProvider;
            }
        );
        return new(brandGameProviders);
    }

    private async Task<HashSet<BrandBetLimit>> GenerateBetLimitsAsync(Brand brand, BrandBetLimitModel[] brandBetLimitModels)
    {
        var betLimits = await _repository.BetLimits.AsQueryable()
            .Where(x => brandBetLimitModels.Select(b => b.Id).Contains(x.Id)).ToArrayAsync();
        var brandBetLimits = betLimits.Select(x =>
            new BrandBetLimit
            {
                BrandId = brand.Id,
                BetLimitId = x.Id,
                BetLimit = x,
                IsDefault = brandBetLimitModels.First(b => b.Id == x.Id).IsDefault
            });
        return new(brandBetLimits);
    }

    private HashSet<BrandCurrency> GenerateBrandCurrencies(Brand brand, string[] currencyCodes)
    {
        var brandCurrencies = _repository.Currencies.AsQueryable()
            .Where(x => currencyCodes.Contains(x.Code)).Select(x =>
                new BrandCurrency
                {
                    BrandId = brand.Id,
                    CurrencyCode = x.Code,
                    BrandCurrencyCode = x.Code
                });
        return new(brandCurrencies);
    }

    private HashSet<BrandDomain> GenerateBrandDomains(Brand brand, string[] domains)
    {
        var brandDomains = domains.Select(x => new BrandDomain
        {
            Url = x,
            BrandId = brand.Id
        });
        return new(brandDomains);
    }

    private HashSet<BrandGroupBetLimit> GenerateBrandGroupBetLimits(Brand brand, Guid brandGroupId,
        bool isSports = false)
    {
        var brandGroupBetLimits = brand.BetLimits.Select(x =>
        {
            var id = Guid.NewGuid();
            return new BrandGroupBetLimit
            {
                Id = id,
                BrandGroupId = brandGroupId,
                BetLimitId = x.BetLimitId,
                GameProviderBetLimits = GenerateGameProviderBetLimits(brand, id, x.BetLimit, isSports)
            };
        });
        return new(brandGroupBetLimits);
    }

    private async Task<HashSet<BrandLanguage>> GenerateBrandLanguagesAsync(Brand brand, BrandLanguageModel[] languageModels)
    {
        var cultureCodes = languageModels.Select(c => c.CultureCode);
        var languages = await _repository.Languages.AsQueryable()
            .Where(x => cultureCodes.Contains(x.CultureCode)).ToArrayAsync();
        var brandLanguages = languages.Select(x =>
            new BrandLanguage
            {
                BrandId = brand.Id,
                CultureCode = x.CultureCode,
                BrandCultureCode = languageModels.FirstOrDefault(bc => bc.CultureCode == x.CultureCode)
                    ?.BrandCultureCode
            });
        return new(brandLanguages);
    }

    private async Task<HashSet<BrandGame>> GenerateWithCreateGamesAsync(Brand brand)
    {
        var gameProviderIds = brand.GameProviders.Select(x => x.GameProviderId).ToArray();

        var brandGames = await _repository.Games.AsQueryable()
            .Where(x => x.AvailableToBrand)
            .Where(x => gameProviderIds.Contains(x.GameProviderId)).ToArrayAsync();

        var games = brandGames.Select(x =>
            new BrandGame
            {
                Id = Guid.NewGuid(),
                BrandId = brand.Id,
                GameId = x.Id,
                Game = x,
                Status = GameStatus.Active,
                Tag = GameTag.None,
                DisplayOrder = 99,
                Rebate = true,
                EffectiveBettingPercentage = 100M
            });
        return new(games);
    }

    private HashSet<BrandGroup> GenerateWithCreateGroups(Brand brand, string editorUsername)
    {
        var groups = (from BrandGroupType type in Enum.GetValues(typeof(BrandGroupType))
                      let id = Guid.NewGuid()
                      let code = type.ToString()
                      let gameIds = type == BrandGroupType.Sports
                          ? brand.Games.Where(x => Consts.Sports.Contains(x.Game.Type)).Select(x => x.GameId)
                              .ToArray()
                          : brand.Games.Where(x => !Consts.Sports.Contains(x.Game.Type)).Select(x => x.GameId)
                              .ToArray()
                      select new BrandGroup
                      {
                          Id = id,
                          Name = code,
                          IsSports = type == BrandGroupType.Sports,
                          BrandId = brand.Id,
                          Description = code,
                          BrandGames = GenerateBrandGroupGames(brand, id, gameIds),
                          BrandGroupBetLimits =
                              GenerateBrandGroupBetLimits(brand, id, type == BrandGroupType.Sports),
                          CreatedByUsername = editorUsername,
                          UpdatedByUsername = editorUsername,
                          CreatedOn = DateTimeOffset.Now,
                          UpdatedOn = DateTimeOffset.Now
                      }).ToList();

        return new(groups);
    }

    private HashSet<BrandGroupBetLimit> GenerateWithUpdateBrandGroupBetLimits(Brand brand, BrandGroup brandGroup,
        BrandGroupGameProviderBetLimitModel[] models)
    {
        var brandGroupBetLimits = brand.BetLimits.Select(x =>
        {
            var brandGroupBetLimit = new BrandGroupBetLimit
            {
                BrandGroup = brandGroup,
                BetLimitId = x.BetLimitId
            };
            brandGroupBetLimit.GameProviderBetLimits =
                GenerateWithUpdateGameProviderBetLimits(brandGroupBetLimit, models);

            return brandGroupBetLimit;
        });
        return new(brandGroupBetLimits);
    }

    private async Task<HashSet<BrandGame>> GenerateWithUpdateGamesAsync(Brand brand,
        BrandGameProviderModel[] brandGameProviderModels)
    {
        var gameProviderIds = brandGameProviderModels.Select(x => x.GameProviderId);

        var brandCurrencyCodes = brand.Currencies.Select(x => x.CurrencyCode).ToArray();

        var games = await _repository.Games.AsQueryable()
            .Where(x => x.AvailableToBrand)
            .Where(x => gameProviderIds.Contains(x.GameProviderId))
            .Where(x => x.GameProvider.Currencies.Any(c => brandCurrencyCodes.Contains(c.CurrencyCode)))
            .ToArrayAsync();

        games = games.Where(x
            => x.AvailableCurrencies.IsNullOrEmpty() ||
            x.AvailableCurrencies.Split(",").Any(c => brandCurrencyCodes.Contains(c))).ToArray();

        var brandGames = games.Select(x =>
        {
            var originBrandGame = brand.Games.FirstOrDefault(g => g.GameId == x.Id);

            var brandGame = new BrandGame
            {
                BrandId = brand.Id,
                GameId = x.Id,
                Game = x,
                Status = GameStatus.Active,
                Tag = GameTag.None,
                DisplayOrder = 99,
                Rebate = true,
                EffectiveBettingPercentage = 100M
            };

            if (originBrandGame == null)
            {
                return brandGame;
            }

            brandGame.Id = originBrandGame.Id;
            brandGame.Status = originBrandGame.Status;
            brandGame.Tag = originBrandGame.Tag;
            brandGame.DisplayOrder = originBrandGame.DisplayOrder;
            brandGame.Rebate = originBrandGame.Rebate;
            brandGame.EffectiveBettingPercentage = originBrandGame.EffectiveBettingPercentage;

            return brandGame;
        });

        return new(brandGames);
    }

    private HashSet<BrandGroup> GenerateWithUpdateGroups(Brand brand, BrandGroupModel[] brandGroupModels, string editorUsername)
    {
        var brandGroups = brandGroupModels.Select(x =>
        {
            var brandGroup = new BrandGroup
            {
                Name = x.Name,
                BrandId = brand.Id,
                Description = x.Description,
                IsSports = x.IsSports,
                CreatedByUsername = editorUsername,
                UpdatedByUsername = editorUsername,
                CreatedOn = DateTimeOffset.Now,
                UpdatedOn = DateTimeOffset.Now
            };
            brandGroup.BrandGames =
                GenerateWithUpdateBrandGroupGames(brand, brandGroup, x.BrandGames?.Select(g => g.GameId));
            brandGroup.BrandGroupBetLimits =
                GenerateWithUpdateBrandGroupBetLimits(brand, brandGroup, x.GameProviderBetLimits);

            return brandGroup;
        });

        return new(brandGroups);
    }
}
