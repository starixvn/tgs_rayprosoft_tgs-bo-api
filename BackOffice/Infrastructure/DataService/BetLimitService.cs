using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.BackOffice.Application.Models.BetLimit;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IBetLimitService
{
    Task CreateBetLimitAsync(CreateBetLimitRequest request);

    Task<BetLimit> GetBetLimitAsync(Expression<Func<BetLimit, bool>> predicate,
            Func<IQueryable<BetLimit>, IQueryable<BetLimit>> preQuery = null);

    IQueryable<BetLimit> GetBetLimitQueries(Func<IQueryable<BetLimit>, IQueryable<BetLimit>> preQuery = null,
        bool tracking = false);

    Task UpdateBetLimitAsync(BetLimit betLimit, UpdateBetLimitRequest request);
}

public class BetLimitService : BaseRepositoryOperations<ITgsRepository>, IBetLimitService
{
    public BetLimitService(ITgsRepository tgsRepository) : base(tgsRepository)
    {
    }

    async Task IBetLimitService.CreateBetLimitAsync(CreateBetLimitRequest request)
    {
        var betLimit = new BetLimit
        {
            CurrencyCode = request.CurrencyCode,
            MinBet = request.MinBet,
            MaxBet = request.MaxBet,
            MaxBetPerMatch = request.MaxBetPerMatch,
            CreatedByUsername = request.EditorUsername,
            UpdatedByUsername = request.EditorUsername,
            CreatedOn = DateTimeOffset.Now,
            UpdatedOn = DateTimeOffset.Now
        };

        await CreateEntityAsync(betLimit);
    }

    Task<BetLimit> IBetLimitService.GetBetLimitAsync(Expression<Func<BetLimit, bool>> predicate,
        Func<IQueryable<BetLimit>, IQueryable<BetLimit>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);

    IQueryable<BetLimit> IBetLimitService.GetBetLimitQueries(
        Func<IQueryable<BetLimit>, IQueryable<BetLimit>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);

    Task IBetLimitService.UpdateBetLimitAsync(BetLimit betLimit, UpdateBetLimitRequest request)
    {
        betLimit.CurrencyCode = request.CurrencyCode;
        betLimit.MinBet = request.MinBet;
        betLimit.MaxBet = request.MaxBet;
        betLimit.MaxBetPerMatch = request.MaxBetPerMatch;
        betLimit.UpdatedByUsername = request.EditorUsername;
        betLimit.UpdatedOn = DateTimeOffset.Now;

        return UpdateEntityAsync(betLimit);
    }
}
