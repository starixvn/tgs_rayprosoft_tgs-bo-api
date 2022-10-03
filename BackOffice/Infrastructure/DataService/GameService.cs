using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Redis;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.Game;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IGameService
{
    Task<Game> GetGameAsync(
        Expression<Func<Game, bool>> predicate,
        Func<IQueryable<Game>, IQueryable<Game>> preQuery =
            null);

    IQueryable<Game> GetGameQueries(
        Func<IQueryable<Game>, IQueryable<Game>> preQuery = null,
        bool tracking = false);

    Task<bool> ExistsAsync(Expression<Func<Game, bool>> predicate);
    Task CreateGamesAsync(GameProvider gameProvider, CreateGamesRequest request);
    Task UpdateGameAsync(Game game);
}

public class GameService : BaseRepositoryOperations<ITgsRepository>, IGameService
{
    private readonly IRedisCacheManager _cacheManager;

    public GameService(ITgsRepository tgsRepository, IRedisCacheManager cacheManager) : base(tgsRepository)
    {
        _cacheManager = cacheManager;
    }

    Task<bool> IGameService.ExistsAsync(Expression<Func<Game, bool>> predicate) =>
        ExistsAsync(predicate);

    async Task IGameService.CreateGamesAsync(GameProvider gameProvider, CreateGamesRequest request)
    {
        var games = GenerateCreateGames(gameProvider, request);

        await CreateEntitiesAsync(games);

        var cacheKeys = new[]
        {
            new CacheKey
            {
                CacheType = RedisCacheType.Games,
                Key = gameProvider.Id
            },
            new CacheKey
            {
                CacheType = RedisCacheType.SubGames,
                Key = gameProvider.Id
            }
        };

        await _cacheManager.ClearCacheByBatchAsync(cacheKeys);
    }

    async Task IGameService.UpdateGameAsync(Game game)
    {
        await UpdateEntityAsync(game);

        var cacheKeys = new[]
        {
            new CacheKey
            {
                CacheType = RedisCacheType.Games,
                Key = game.GameProviderId
            },
            new CacheKey
            {
                CacheType = RedisCacheType.SubGames,
                Key = game.GameProviderId
            }
        };

        await _cacheManager.ClearCacheByBatchAsync(cacheKeys);
    }

    Task<Game> IGameService.GetGameAsync(Expression<Func<Game, bool>> predicate,
        Func<IQueryable<Game>, IQueryable<Game>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);

    IQueryable<Game> IGameService.
        GetGameQueries(Func<IQueryable<Game>, IQueryable<Game>> preQuery, bool tracking) =>
        GetQueriesAsync(preQuery, tracking);

    private static HashSet<Game> GenerateCreateGames(GameProvider gameProvider,
        CreateGamesRequest request)
    {
        var maxNumberStr = gameProvider.Games.Count > 0
            ? gameProvider.Games.Max(x => x.Id).ToString()
            : "00000";
        var maxNumber = int.Parse(maxNumberStr[^5..]);

        var currentMaxGameNumber = gameProvider.UniqueId * (int)Math.Pow(10, 5) + maxNumber;

        var games = request.Games.Select((x, index) =>
        {
            var originGame = gameProvider.Games.FirstOrDefault(g => g.Id == x.Id);
            var id = originGame == null ? currentMaxGameNumber + 1 + index : x.Id;
            return new Game
            {
                Id = id,
                GameProviderId = gameProvider.Id,
                SubGameProviderCode = x.SubGpCode,
                Code = x.Code,
                Name = x.Name,
                Status = x.Status,
                ExternalId = x.ExternalId,
                Description = x.Description,
                EndpointPath = x.EndpointPath,
                RuleUrl = x.RuleUrl,
                Type = x.Type,
                PlatformType = x.PlatformType,
                SubGame = x.SubGame,
                AvailableToBrand = x.AvailableToBrand,
                HasTurnoverOnTieBet = x.HasTurnoverOnTieBet,
                Html5 = x.Html5,
                AvailableCurrencies = x.AvailableCurrencies,
                CreatedByUsername = originGame?.CreatedByUsername ?? request.EditorUsername,
                UpdatedByUsername = originGame?.UpdatedByUsername ?? request.EditorUsername,
                CreatedOn = originGame?.CreatedOn ?? DateTimeOffset.Now,
                UpdatedOn = originGame?.UpdatedOn ?? DateTimeOffset.Now
            };
        });

        return new(games);
    }
}