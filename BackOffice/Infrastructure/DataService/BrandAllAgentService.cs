using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IBrandAllAgentService
{
    Task CreateBrandAllAgentAsync(BrandAllAgent brandAllAgent);

    Task DeleteBrandAllAgentAsync(BrandAllAgent brandAllAgent);

    Task<bool> ExistsAsync(Expression<Func<BrandAllAgent, bool>> predicate);

    Task<BrandAllAgent> GetBrandAllAgentAsync(Expression<Func<BrandAllAgent, bool>> predicate,
                    Func<IQueryable<BrandAllAgent>, IQueryable<BrandAllAgent>> preQuery = null);

    IQueryable<BrandAllAgent> GetBrandAllAgentQueries(
        Func<IQueryable<BrandAllAgent>, IQueryable<BrandAllAgent>> preQuery = null,
        bool tracking = false);

    Task UpdateBrandAllAgentSwitchAsync(BrandAllAgent brandAllAgent);
}

public class BrandAllAgentService(ITgsRepository tgsRepository) : BaseRepositoryOperations<ITgsRepository>(tgsRepository),
    IBrandAllAgentService
{
    public Task CreateBrandAllAgentAsync(BrandAllAgent brandAllAgent)
        => CreateEntityAsync(brandAllAgent);

    public Task DeleteBrandAllAgentAsync(BrandAllAgent brandAllAgent) => DeleteEntityAsync(brandAllAgent);

    public Task<bool> ExistsAsync(Expression<Func<BrandAllAgent, bool>> predicate) => base.ExistsAsync(predicate);

    public Task<BrandAllAgent> GetBrandAllAgentAsync(Expression<Func<BrandAllAgent, bool>> predicate,
        Func<IQueryable<BrandAllAgent>, IQueryable<BrandAllAgent>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);

    public IQueryable<BrandAllAgent> GetBrandAllAgentQueries(
                    Func<IQueryable<BrandAllAgent>, IQueryable<BrandAllAgent>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);

    public Task UpdateBrandAllAgentSwitchAsync(BrandAllAgent brandAllAgent) => UpdateEntityAsync(brandAllAgent);
}
