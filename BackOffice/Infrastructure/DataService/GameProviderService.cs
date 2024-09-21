using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Redis;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.GameProvider;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IGameProviderService
{
    Task ChangeStatusAsync(GameProvider gameProvider, UpdateGameProviderStatusRequest request);

    Task CreateGameProviderAsync(CreateGameProviderRequest request);

    Task<bool> ExistsAsync(Expression<Func<GameProvider, bool>> predicate);

    Task<GameProvider> GetGameProviderAsync(
        Expression<Func<GameProvider, bool>> predicate,
        Func<IQueryable<GameProvider>, IQueryable<GameProvider>> preQuery =
            null);

    IQueryable<GameProvider> GetGameProviderQueries(
        Func<IQueryable<GameProvider>, IQueryable<GameProvider>> preQuery = null,
        bool tracking = false);

    Task UpdateGameProviderAsync(GameProvider gameProvider, UpdateGameProviderRequest request);
}

public class GameProviderService(ITgsRepository tgsRepository, IRedisCacheManager cacheManager) : BaseRepositoryOperations<ITgsRepository>(tgsRepository), IGameProviderService
{
    private readonly IRedisCacheManager _cacheManager = cacheManager;
    private readonly ITgsRepository _repository = tgsRepository;

    async Task IGameProviderService.ChangeStatusAsync(GameProvider gameProvider,
        UpdateGameProviderStatusRequest request)
    {
        gameProvider.Status = request.Status;
        gameProvider.UpdatedByUsername = request.EditorUsername;
        gameProvider.UpdatedOn = DateTimeOffset.Now;

        await UpdateEntityAsync(gameProvider);

        await _cacheManager.ClearCacheAsync(RedisCacheType.GameProvider, gameProvider.Id);
    }

    async Task IGameProviderService.CreateGameProviderAsync(CreateGameProviderRequest request)
    {
        var maxUniqueId = _repository.GameProviders.Max(x => x.UniqueId);

        var gameProvider = new GameProvider
        {
            Code = request.Code,
            Name = request.Name,
            Status = request.Status,
            AutoCloseRound = request.AutoCloseRound,
            SupportTestPlayer = request.SupportTestPlayer,
            AllowKickPlayer = request.AllowKickPlayer,
            PlaceBetTimeout = request.PlaceBetTimeout,
            SettleBetTimeout = request.SettleBetTimeout,
            TransferWallet = request.IsTransferWallet,
            UniqueId = (ushort)(maxUniqueId + 1),
            Description = request.Description,
            CreatedByUsername = request.EditorUsername,
            UpdatedByUsername = request.EditorUsername,
            CreatedOn = DateTimeOffset.Now,
            UpdatedOn = DateTimeOffset.Now
        };

        gameProvider.Currencies = await GenerateCurrenciesAsync(gameProvider, request);
        gameProvider.Languages = await GenerateLanguagesAsync(gameProvider, request);
        gameProvider.BetLimits = GenerateNewBetLimits(gameProvider, request);
        await CreateEntityAsync(gameProvider);
    }

    Task<bool> IGameProviderService.ExistsAsync(Expression<Func<GameProvider, bool>> predicate) =>
        ExistsAsync(predicate);

    Task<GameProvider> IGameProviderService.GetGameProviderAsync(
        Expression<Func<GameProvider, bool>> predicate,
        Func<IQueryable<GameProvider>, IQueryable<GameProvider>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);

    IQueryable<GameProvider> IGameProviderService.GetGameProviderQueries(
        Func<IQueryable<GameProvider>, IQueryable<GameProvider>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);

    async Task IGameProviderService.UpdateGameProviderAsync(
        GameProvider gameProvider, UpdateGameProviderRequest request)
    {
        gameProvider.Name = request.Name;
        gameProvider.AutoCloseRound = request.AutoCloseRound;
        gameProvider.SupportTestPlayer = request.SupportTestPlayer;
        gameProvider.AllowKickPlayer = request.AllowKickPlayer;
        gameProvider.PlaceBetTimeout = request.PlaceBetTimeout;
        gameProvider.SettleBetTimeout = request.SettleBetTimeout;
        gameProvider.TransferWallet = request.IsTransferWallet;
        gameProvider.Description = request.Description;
        gameProvider.UpdatedByUsername = request.EditorUsername;
        gameProvider.UpdatedOn = DateTimeOffset.Now;

        await using var tran = await _repository.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        await DeleteEntitiesAsync(gameProvider.Currencies);
        await DeleteEntitiesAsync(gameProvider.Languages);

        gameProvider.Currencies = await GenerateCurrenciesAsync(gameProvider, request);
        gameProvider.Languages = await GenerateLanguagesAsync(gameProvider, request);
        gameProvider.BetLimits = GenerateBetLimits(gameProvider, request);

        await UpdateEntityAsync(gameProvider);
        await CreateEntitiesAsync(GenerateNewBetLimits(gameProvider, request));

        await tran.CommitAsync();

        var cacheKeys = new[]
        {
            new CacheKey
            {
                CacheType = RedisCacheType.Currencies,
                Key = gameProvider.Id
            },
            new CacheKey
            {
                CacheType = RedisCacheType.Languages,
                Key = gameProvider.Id
            },
            new CacheKey
            {
                CacheType = RedisCacheType.GameProvider,
                Key = gameProvider.Id
            },
            new CacheKey
            {
                CacheType = RedisCacheType.GameProviderIdByCode,
                Key = gameProvider.Code
            },
            new CacheKey
            {
                CacheType = RedisCacheType.GameProviderConfiguration,
                Key = gameProvider.Id
            },
            new CacheKey
            {
                CacheType = RedisCacheType.GameProviderLanguages,
                Key = gameProvider.Id
            },
            new CacheKey
            {
                CacheType = RedisCacheType.GameProviderCurrencies,
                Key = gameProvider.Id
            },
            new CacheKey
            {
                CacheType = RedisCacheType.Localization,
                Key = gameProvider.Id
            }
        };

        _ = await _cacheManager.ClearCacheByBatchAsync(cacheKeys);
    }

    private static HashSet<GameProviderBetLimit> GenerateBetLimits(GameProvider gameProvider,
        GameProviderRequest request) =>
        request.BetLimits.Where(x => x.Id is not null).Select(x => new GameProviderBetLimit
        {
            Id = x.Id.Value,
            GameProviderId = gameProvider.Id,
            CurrencyCode = x.CurrencyCode,
            MinBet = x.MinBet,
            MaxBet = x.MaxBet,
            MaxBetPerMatch = x.MaxBetPerMatch,
            ExternalSetting = x.ExternalSetting
        }).ToHashSet();

    private static HashSet<GameProviderBetLimit> GenerateNewBetLimits(GameProvider gameProvider,
        GameProviderRequest request) =>
        request.BetLimits.Where(x => x.Id is null).Select(x => new GameProviderBetLimit
        {
            GameProviderId = gameProvider.Id,
            CurrencyCode = x.CurrencyCode,
            MinBet = x.MinBet,
            MaxBet = x.MaxBet,
            MaxBetPerMatch = x.MaxBetPerMatch,
            ExternalSetting = x.ExternalSetting
        }).ToHashSet();

    private async Task<HashSet<GameProviderCurrency>> GenerateCurrenciesAsync(GameProvider gameProvider,
                GameProviderRequest request)
    {
        var currencyCodes = request.Currencies.Select(x => x.Code);
        var currencies = await _repository.Currencies.AsQueryable().Where(x => currencyCodes.Contains(x.Code))
            .ToArrayAsync();
        var gameProviderCurrencies = currencies.Select(x => new GameProviderCurrency
        {
            GameProviderId = gameProvider.Id,
            CurrencyCode = x.Code,
            GameProviderCurrencyCode =
                request.Currencies.First(c => c.Code == x.Code).GameProviderCurrencyCode
        });
        return new(gameProviderCurrencies);
    }

    private async Task<HashSet<GameProviderLanguage>> GenerateLanguagesAsync(GameProvider gameProvider,
        GameProviderRequest request)
    {
        var cultureCodes = request.Languages.Select(x => x.CultureCode);
        var languages = await _repository.Languages.AsQueryable().Where(x => cultureCodes.Contains(x.CultureCode))
            .ToArrayAsync();
        var gameProviderLanguages = languages.Select(x => new GameProviderLanguage
        {
            GameProviderId = gameProvider.Id,
            CultureCode = x.CultureCode,
            GameProviderCultureCode =
                request.Languages.First(c => c.CultureCode == x.CultureCode).GameProviderCultureCode
        });
        return new(gameProviderLanguages);
    }
}
