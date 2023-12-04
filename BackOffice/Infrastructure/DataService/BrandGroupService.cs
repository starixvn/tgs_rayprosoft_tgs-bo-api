using System;
using System.Linq;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IBrandGroupService
{
    IQueryable<BrandGroup> GetBrandGroupsQueries(
        Func<IQueryable<BrandGroup>, IQueryable<BrandGroup>> preQuery = null,
        bool tracking = false);
}

public class BrandGroupService(ITgsRepository tgsRepository) : BaseRepositoryOperations<ITgsRepository>(tgsRepository), IBrandGroupService
{
    IQueryable<BrandGroup> IBrandGroupService.GetBrandGroupsQueries(
        Func<IQueryable<BrandGroup>, IQueryable<BrandGroup>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);
}
