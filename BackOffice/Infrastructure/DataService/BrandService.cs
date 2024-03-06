using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Infrastructures.Redis;
using TGS.Core.Models;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Domain;
using IsolationLevel = System.Data.IsolationLevel;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public class BrandService(
    ITgsRepository tgsRepository,
    IRedisCacheManager cacheManager)
#pragma warning disable CS9107 // 參數會擷取至包含類型的狀態，且其值也會傳遞給基礎建構函式。值也可能由基礎類別擷取。
    : BaseRepositoryOperations<ITgsRepository>(tgsRepository)
#pragma warning restore CS9107 // 參數會擷取至包含類型的狀態，且其值也會傳遞給基礎建構函式。值也可能由基礎類別擷取。
    , IBrandService
{
    public async ValueTask<Guid> CreateBrandAsync(CreateBrandInfo createInfo, CancellationToken cancellationToken = default)
    {
        var brand = await tgsRepository.Brands.FirstAsync(x => x.Id == createInfo.BrandId, cancellationToken);

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var languages = await GenerateBrandLanguagesAsync(brand.Id, createInfo.Languages.ToArray());
        var currencies = GenerateBrandCurrencies(brand.Id, createInfo.CurrencyCodes.ToArray());
        var gameProviders = await GenerateAddBrandGameProvidersAsync(brand.Id, [.. languages], createInfo.GameProviders.ToArray());
        var betLimits = await GenerateBetLimitsAsync(brand.Id, createInfo.BetLimits.ToArray());
        var games = await GenerateWithCreateGamesAsync(brand.Id, [.. gameProviders]);
        var brandGroups = GenerateWithCreateGroups(
            brand.Id,
            [.. games],
            [.. betLimits],
            [.. gameProviders],
            createInfo.Modifier);
        var domains = GenerateBrandDomains(brand.Id, createInfo.Domains.ToArray());

        await CreateEntitiesAsync(languages);
        await CreateEntitiesAsync(currencies);
        await CreateEntitiesAsync(gameProviders);
        await CreateEntitiesAsync(betLimits);
        await CreateEntitiesAsync(games);
        await CreateEntitiesAsync(brandGroups);
        await CreateEntitiesAsync(domains);

        //await UpdateEntityAsync(brand);

        var platformCredit = new PlatformCredit
        {
            BrandId = brand.Id,
            Credit = 0,
            Balance = 0,
            CreatedByUsername = createInfo.Modifier,
            UpdatedByUsername = createInfo.Modifier,
            CreatedOn = DateTimeOffset.Now,
            UpdatedOn = DateTimeOffset.Now
        };

        await CreateEntityAsync(platformCredit);

        var vipLevel = new VipLevel
        {
            BrandId = brand.Id,
            Level = 1,
            CreatedByUsername = createInfo.Modifier,
            UpdatedByUsername = createInfo.Modifier,
            CreatedOn = DateTimeOffset.Now,
            UpdatedOn = DateTimeOffset.Now
        };

        await CreateEntityAsync(vipLevel);

        scope.Complete();

        return brand.Id;
    }

    public Task<bool> ExistsAsync(Expression<Func<Brand, bool>> predicate)
        => base.ExistsAsync(predicate);

    public Task<Brand> GetBrandAsync(
        Expression<Func<Brand, bool>> predicate,
        Func<IQueryable<Brand>, IQueryable<Brand>> preQuery)
        => GetEntityFirstOrDefaultAsync(predicate, preQuery);

    public IQueryable<Brand> GetBrandQueries(
        Func<IQueryable<Brand>, IQueryable<Brand>> preQuery,
        bool tracking)
        => GetQueriesAsync(preQuery, tracking);

    public async Task SyncBrandGamesAsync(Guid brandId)
    {
        var brand = await tgsRepository.Brands.AsQueryable().FirstOrDefaultAsync(x => x.Id == brandId);

        var brandCurrencies = brand.Currencies.Select(bc => bc.CurrencyCode);

        var baseGames = await tgsRepository.Games.AsQueryable()
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

        await cacheManager.ClearCacheAsync(RedisCacheType.BrandGames, brand.Id, true);
    }

    public async Task UpdateBrandAsync(Guid brandId, UpdateBrandRequest request)
    {
        var brand = await tgsRepository.Brands.FirstAsync(x => x.Id == brandId);

        await using var tran = await tgsRepository.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        await DeleteEntitiesAsync(brand.BetLimits);
        await DeleteEntitiesAsync(brand.Languages);
        await DeleteEntitiesAsync(brand.Currencies);
        await DeleteEntitiesAsync(brand.BrandGroups);
        await DeleteEntitiesAsync(brand.Domains);

        brand.Languages = await GenerateBrandLanguagesAsync(brand.Id, request.Languages);
        brand.Currencies = GenerateBrandCurrencies(brand.Id, request.CurrencyCodes);
        brand.BetLimits = await GenerateBetLimitsAsync(brand.Id, request.BetLimits);
        brand.Games = await GenerateWithUpdateGamesAsync(brand, request.GameProviders);
        brand.GameProviders = GenerateBrandGameProviders(brand, request.GameProviders);
        brand.BrandGroups = GenerateWithUpdateGroups(brand, request.BrandGroups, request.EditorUsername);
        brand.Domains = GenerateBrandDomains(brand.Id, request.Domains);

        var gameProviders = await GenerateAddBrandGameProvidersAsync(
            brand.Id,
            [.. brand.Languages],
            request.GameProviders).ConfigureAwait(false);

        await UpdateEntityAsync(brand);
        await CreateEntitiesAsync(gameProviders);

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

        _ = await cacheManager.ClearCacheByBatchAsync(cacheKeys);
    }

    private static HashSet<BrandDomain> GenerateBrandDomains(Guid brandId, string[] domains)
    {
        var brandDomains = domains.Select(x => new BrandDomain
        {
            Url = x,
            BrandId = brandId
        });
        return new(brandDomains);
    }

    private static HashSet<BrandGameProvider> GenerateBrandGameProviders(
            Brand brand,
            BrandGameProviderModel[] brandGameProviderModels)
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

    private static HashSet<BrandGroupBetLimit> GenerateBrandGroupBetLimits(BrandBetLimit[] brandBetLimits, BrandGameProvider[] brandGameProviders, Guid brandGroupId,
        bool isSports = false)
    {
        var brandGroupBetLimits = brandBetLimits.Select(x =>
        {
            var id = Guid.NewGuid();
            return new BrandGroupBetLimit
            {
                Id = id,
                BrandGroupId = brandGroupId,
                BetLimitId = x.BetLimitId,
                GameProviderBetLimits = GenerateGameProviderBetLimits(brandGameProviders, id, x.BetLimit, isSports)
            };
        });
        return new(brandGroupBetLimits);
    }

    private static HashSet<BrandGroupGame> GenerateBrandGroupGames(BrandGame[] brandGames, Guid brandGroupId,
            IEnumerable<int> brandGameIds)
    {
        var brandGroupGames = brandGameIds.Select(brandGameId => new BrandGroupGame
        {
            BrandGroupId = brandGroupId,
            BrandGame = brandGames.First(g => g.GameId == brandGameId)
        });
        return new(brandGroupGames);
    }

    private static HashSet<BrandGroupGameProviderBetLimit> GenerateGameProviderBetLimits(BrandGameProvider[] brandGameProviders,
        Guid brandGroupBetLimitId, BetLimit betLimit, bool isSports)
    {
        bool BetLimitRangeFunc(GameProviderBetLimit gb) =>
            gb.CurrencyCode == betLimit.CurrencyCode && betLimit.MinBet <= gb.MinBet &&
            betLimit.MaxBet >= gb.MaxBet;

        bool BetLimitIsSportsFunc(GameProviderBetLimit b) => betLimit.MaxBetPerMatch >= b.MaxBetPerMatch;

        var brandGroupGameProviderBetLimits = brandGameProviders
            .Where(x => x.GameProvider.BetLimits.Any(BetLimitRangeFunc))
            .Where(isSports, x => x.GameProvider.BetLimits.Any(BetLimitIsSportsFunc))
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

    private static HashSet<BrandGroup> GenerateWithCreateGroups(Guid brandId, BrandGame[] brandGames, BrandBetLimit[] brandBetLimits, BrandGameProvider[] brandGameProviders, string editorUsername)
    {
        var groups = (from BrandGroupType type in Enum.GetValues(typeof(BrandGroupType))
                      let id = Guid.NewGuid()
                      let code = type.ToString()
                      let gameIds = type == BrandGroupType.Sports
                          ? brandGames.Where(x => Consts.Sports.Contains(x.Game.Type)).Select(x => x.GameId)
                              .ToArray()
                          : brandGames.Where(x => !Consts.Sports.Contains(x.Game.Type)).Select(x => x.GameId)
                              .ToArray()
                      select new BrandGroup
                      {
                          Id = id,
                          Name = code,
                          IsSports = type == BrandGroupType.Sports,
                          BrandId = brandId,
                          Description = code,
                          BrandGames = GenerateBrandGroupGames(brandGames, id, gameIds),
                          BrandGroupBetLimits =
                              GenerateBrandGroupBetLimits(brandBetLimits, brandGameProviders, id, type == BrandGroupType.Sports),
                          CreatedByUsername = editorUsername,
                          UpdatedByUsername = editorUsername,
                          CreatedOn = DateTimeOffset.Now,
                          UpdatedOn = DateTimeOffset.Now
                      }).ToList();

        groups.ForEach(x => x.BrandGames.ForEach(b =>
        {
            b.BrandGroup = x;
        }));

        return new(groups);
    }

    private static HashSet<BrandGroupBetLimit> GenerateWithUpdateBrandGroupBetLimits(
        Brand brand,
        BrandGroup brandGroup,
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
        return new(brandGroupGames ?? []);
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

        return new(brandGroupGameProviderBetLimits ?? []);
    }

    private static HashSet<BrandGroup> GenerateWithUpdateGroups(Brand brand, BrandGroupModel[] brandGroupModels, string editorUsername)
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

    private async Task<HashSet<BrandGameProvider>> GenerateAddBrandGameProvidersAsync(
        Guid brandId,
        BrandLanguage[] brandLanguages,
        BrandGameProviderModel[] brandGameProviderModels)
    {
        var gameProviderIds =
            brandGameProviderModels.Where(x => x.Id == null).Select(x => x.GameProviderId).ToArray();

        var gameProviders = await tgsRepository.GameProviders.AsQueryable()
            .Where(x => gameProviderIds.Contains(x.Id)).ToArrayAsync();
        var brandGameProviders = gameProviders.Select(x =>
            {
                var requestBrandGameProvider = brandGameProviderModels.First(m => m.GameProviderId == x.Id);
                var brandGameProviderId = Guid.NewGuid();

                var newBrandGameProvider = new BrandGameProvider
                {
                    Id = brandGameProviderId,
                    BrandId = brandId,
                    GameProviderId = x.Id,
                    GameProvider = x,
                    DefaultLanguageCode = brandLanguages.FirstOrDefault()?.CultureCode,
                    GameProviderConfigurationId = requestBrandGameProvider.ConfigurationId,
                    DesktopQueryString = requestBrandGameProvider.DesktopQueryString,
                    MobileQueryString = requestBrandGameProvider.MobileQueryString,
                    Schedules = [
                        new()
                        {
                            Id = Guid.NewGuid(),
                            BrandGameProviderId = brandGameProviderId,
                            From = DateTimeOffset.Now,
                            To = DateTimeOffset.Now.AddDays(7),
                            DayOfWeek = DayOfWeek.Monday,
                            IsDefault = true,
                            Status = SwitchStatus.Disabled
                        }
                    ]
                };

                return newBrandGameProvider;
            }
        );
        return new(brandGameProviders);
    }

    private async Task<HashSet<BrandBetLimit>> GenerateBetLimitsAsync(
        Guid brandId,
        BrandBetLimitModel[] brandBetLimitModels)
    {
        var betLimits = await tgsRepository.BetLimits.AsQueryable()
            .Where(x => brandBetLimitModels.Select(b => b.Id).Contains(x.Id)).ToArrayAsync();
        var brandBetLimits = betLimits.Select(x =>
            new BrandBetLimit
            {
                BrandId = brandId,
                BetLimitId = x.Id,
                BetLimit = x,
                IsDefault = brandBetLimitModels.First(b => b.Id == x.Id).IsDefault
            });
        return new(brandBetLimits);
    }

    private HashSet<BrandCurrency> GenerateBrandCurrencies(
        Guid brandId,
        string[] currencyCodes)
    {
        var brandCurrencies = tgsRepository.Currencies.AsQueryable()
            .Where(x => currencyCodes.Contains(x.Code)).Select(x =>
                new BrandCurrency
                {
                    BrandId = brandId,
                    CurrencyCode = x.Code,
                    BrandCurrencyCode = x.Code
                });
        return new(brandCurrencies);
    }

    private async Task<HashSet<BrandLanguage>> GenerateBrandLanguagesAsync(
        Guid brandId,
        BrandLanguageModel[] languageModels)
    {
        var cultureCodes = languageModels.Select(c => c.CultureCode);
        var languages = await tgsRepository.Languages.AsQueryable()
            .Where(x => cultureCodes.Contains(x.CultureCode)).ToArrayAsync();
        var brandLanguages = languages.Select(x =>
            new BrandLanguage
            {
                BrandId = brandId,
                CultureCode = x.CultureCode,
                BrandCultureCode = languageModels.FirstOrDefault(bc => bc.CultureCode == x.CultureCode)
                    ?.BrandCultureCode
            });
        return new(brandLanguages);
    }

    private async Task<HashSet<BrandGame>> GenerateWithCreateGamesAsync(Guid brandId, BrandGameProvider[] brandGameProviders)
    {
        var gameProviderIds = brandGameProviders.Select(x => x.GameProviderId).ToArray();

        var brandGames = await tgsRepository.Games.AsQueryable()
            .Where(x => x.AvailableToBrand)
            .Where(x => gameProviderIds.Contains(x.GameProviderId)).ToArrayAsync();

        var games = brandGames.Select(x =>
            new BrandGame
            {
                Id = Guid.NewGuid(),
                BrandId = brandId,
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

    private async Task<HashSet<BrandGame>> GenerateWithUpdateGamesAsync(
        Brand brand,
        BrandGameProviderModel[] brandGameProviderModels)
    {
        var gameProviderIds = brandGameProviderModels.Select(x => x.GameProviderId);

        var brandCurrencyCodes = brand.Currencies.Select(x => x.CurrencyCode).ToArray();

        var games = await tgsRepository.Games.AsQueryable()
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
}
