using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IBrandAllAgentGameTypeConditionService
{
    Task CreateBrandAllAgentGameTypeConditionsAsync(BrandAllAgentGameTypeCondition[] brandAllAgents);

    Task DeleteBrandAllAgentGameTypeConditionsAsync(BrandAllAgentGameTypeCondition[] brandAllAgents);

    Task<BrandAllAgentGameTypeCondition> GetBrandAllAgentGameTypeConditionAsync(
                Expression<Func<BrandAllAgentGameTypeCondition, bool>> predicate,
        Func<IQueryable<BrandAllAgentGameTypeCondition>, IQueryable<BrandAllAgentGameTypeCondition>> preQuery =
            null);

    IQueryable<BrandAllAgentGameTypeCondition> GetBrandAllAgentGameTypeConditionQueries(
        Func<IQueryable<BrandAllAgentGameTypeCondition>, IQueryable<BrandAllAgentGameTypeCondition>> preQuery =
            null,
        bool tracking = false);

    Task UpdateBrandAllAgentGameTypeConditionsAsync(BrandAllAgentGameTypeCondition[] brandAllAgents);
}

public class BrandAllAgentGameTypeConditionService(ITgsRepository tgsRepository) : BaseRepositoryOperations<ITgsRepository>(tgsRepository),
    IBrandAllAgentGameTypeConditionService
{
    public Task CreateBrandAllAgentGameTypeConditionsAsync(BrandAllAgentGameTypeCondition[] brandAllAgents)
        => CreateEntitiesAsync(brandAllAgents);

    public Task DeleteBrandAllAgentGameTypeConditionsAsync(BrandAllAgentGameTypeCondition[] brandAllAgents)
        => DeleteEntitiesAsync(brandAllAgents);

    public Task<BrandAllAgentGameTypeCondition> GetBrandAllAgentGameTypeConditionAsync(
        Expression<Func<BrandAllAgentGameTypeCondition, bool>> predicate,
        Func<IQueryable<BrandAllAgentGameTypeCondition>, IQueryable<BrandAllAgentGameTypeCondition>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);

    public IQueryable<BrandAllAgentGameTypeCondition> GetBrandAllAgentGameTypeConditionQueries(
                Func<IQueryable<BrandAllAgentGameTypeCondition>, IQueryable<BrandAllAgentGameTypeCondition>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);

    public Task UpdateBrandAllAgentGameTypeConditionsAsync(BrandAllAgentGameTypeCondition[] brandAllAgents)
        => UpdateEntitiesAsync(brandAllAgents);
}
