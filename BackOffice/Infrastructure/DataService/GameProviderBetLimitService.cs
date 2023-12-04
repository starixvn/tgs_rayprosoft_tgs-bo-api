using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IGameProviderBetLimitService
{
    Task CreateGameProviderBetLimitAsync(CreateGameProviderBetLimitRequest request);

    Task<GameProviderBetLimit> GetGameProviderBetLimitAsync(
            Expression<Func<GameProviderBetLimit, bool>> predicate,
        Func<IQueryable<GameProviderBetLimit>, IQueryable<GameProviderBetLimit>> preQuery =
            null);

    IQueryable<GameProviderBetLimit> GetGameProviderBetLimitQueries(
        Func<IQueryable<GameProviderBetLimit>, IQueryable<GameProviderBetLimit>> preQuery = null,
        bool tracking = false);

    Task<string[]> GetInUseBrandsAsync(Guid id);

    Task UpdateGameProviderBetLimitAsync(GameProviderBetLimit gameProviderBetLimit,
            UpdateGameProviderBetLimitRequest request);
}

public class GameProviderBetLimitService(ITgsRepository tgsRepository) : BaseRepositoryOperations<ITgsRepository>(tgsRepository),
    IGameProviderBetLimitService
{
    private readonly ITgsRepository _repository = tgsRepository;

    Task IGameProviderBetLimitService.CreateGameProviderBetLimitAsync(CreateGameProviderBetLimitRequest request)
    {
        var betLimit = new GameProviderBetLimit
        {
            GameProviderId = request.GameProviderId,
            CurrencyCode = request.CurrencyCode,
            MinBet = request.MinBet,
            MaxBet = request.MaxBet,
            MaxBetPerMatch = request.MaxBetPerMatch,
            ExternalSetting = request.ExternalSetting
        };

        return CreateEntityAsync(betLimit);
    }

    Task<GameProviderBetLimit> IGameProviderBetLimitService.GetGameProviderBetLimitAsync(
        Expression<Func<GameProviderBetLimit, bool>> predicate,
        Func<IQueryable<GameProviderBetLimit>, IQueryable<GameProviderBetLimit>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);

    IQueryable<GameProviderBetLimit> IGameProviderBetLimitService.GetGameProviderBetLimitQueries(
        Func<IQueryable<GameProviderBetLimit>, IQueryable<GameProviderBetLimit>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);

    Task<string[]> IGameProviderBetLimitService.GetInUseBrandsAsync(Guid id) =>
        _repository.BrandGroupGameProviderBetLimits.AsQueryable()
            .Where(x => x.GameProviderBetLimitId == id)
            .Select(x => x.BrandGroupBetLimit.BrandGroup.Brand.Name).Distinct().ToArrayAsync();

    Task IGameProviderBetLimitService.UpdateGameProviderBetLimitAsync(GameProviderBetLimit gameProviderBetLimit,
        UpdateGameProviderBetLimitRequest request)
    {
        gameProviderBetLimit.MinBet = request.MinBet;
        gameProviderBetLimit.MaxBet = request.MaxBet;
        gameProviderBetLimit.MaxBetPerMatch = request.MaxBetPerMatch;
        gameProviderBetLimit.ExternalSetting = request.ExternalSetting;

        return UpdateEntityAsync(gameProviderBetLimit);
    }
}
