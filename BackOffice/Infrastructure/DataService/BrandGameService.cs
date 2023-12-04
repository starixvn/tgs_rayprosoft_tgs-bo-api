using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IBrandGameService
{
    Task<BrandGame> GetBrandGameAsync(
        Expression<Func<BrandGame, bool>> predicate,
        Func<IQueryable<BrandGame>, IQueryable<BrandGame>> preQuery =
            null);

    IQueryable<BrandGame> GetBrandGameQueries(
        Func<IQueryable<BrandGame>, IQueryable<BrandGame>> preQuery = null,
        bool tracking = false);

    Task UpdateBrandGameAsync(BrandGame game);

    Task UpdateBrandGamesAsync(IEnumerable<BrandGame> brandGames);
}

public class BrandGameService(ITgsRepository tgsRepository) : BaseRepositoryOperations<ITgsRepository>(tgsRepository), IBrandGameService
{
    Task<BrandGame> IBrandGameService.GetBrandGameAsync(Expression<Func<BrandGame, bool>> predicate,
        Func<IQueryable<BrandGame>, IQueryable<BrandGame>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);

    IQueryable<BrandGame> IBrandGameService.GetBrandGameQueries(
        Func<IQueryable<BrandGame>, IQueryable<BrandGame>> preQuery, bool tracking) =>
        GetQueriesAsync(preQuery, tracking);

    Task IBrandGameService.UpdateBrandGameAsync(BrandGame game) => UpdateEntityAsync(game);

    async Task IBrandGameService.UpdateBrandGamesAsync(IEnumerable<BrandGame> brandGames) =>
                    await UpdateEntitiesAsync(brandGames);
}
