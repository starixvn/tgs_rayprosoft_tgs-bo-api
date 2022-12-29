using System;
using System.Linq;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IBrandBetLimitService
{
    IQueryable<BrandBetLimit> GetBrandBetLimitsQueries(
        Func<IQueryable<BrandBetLimit>, IQueryable<BrandBetLimit>> preQuery = null,
        bool tracking = false);
}

public class BrandBetLimitService : BaseRepositoryOperations<ITgsRepository>, IBrandBetLimitService
{
    public BrandBetLimitService(ITgsRepository tgsRepository) : base(tgsRepository)
    {
    }

    IQueryable<BrandBetLimit> IBrandBetLimitService.GetBrandBetLimitsQueries(
        Func<IQueryable<BrandBetLimit>, IQueryable<BrandBetLimit>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);
}
