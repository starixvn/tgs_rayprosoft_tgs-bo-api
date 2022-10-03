using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IBrandAllAgentGameTypeService
{
    Task<BrandAllAgentGameType> GetBrandAllAgentGameTypeAsync(
        Expression<Func<BrandAllAgentGameType, bool>> predicate,
        Func<IQueryable<BrandAllAgentGameType>, IQueryable<BrandAllAgentGameType>> preQuery = null);

    IQueryable<BrandAllAgentGameType> GetBrandAllAgentGameTypeQueries(
        Func<IQueryable<BrandAllAgentGameType>, IQueryable<BrandAllAgentGameType>> preQuery = null,
        bool tracking = false);

    Task UpdateBrandAllAgentGameTypeSwitchAsync(BrandAllAgentGameType brandAllAgent);

    Task CreateBrandAllAgentGameTypeAsync(BrandAllAgentGameType brandAllAgent);
    Task DeleteBrandAllAgentGameTypeAsync(BrandAllAgentGameType brandAllAgent);
}

public class BrandAllAgentGameTypeService : BaseRepositoryOperations<ITgsRepository>,
    IBrandAllAgentGameTypeService
{
    private readonly ITgsRepository _repository;

    public BrandAllAgentGameTypeService(ITgsRepository tgsRepository) : base(tgsRepository)
        => _repository = tgsRepository;

    public Task CreateBrandAllAgentGameTypeAsync(BrandAllAgentGameType brandAllAgent)
        => CreateEntityAsync(brandAllAgent);

    public IQueryable<BrandAllAgentGameType> GetBrandAllAgentGameTypeQueries(
        Func<IQueryable<BrandAllAgentGameType>, IQueryable<BrandAllAgentGameType>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);

    public Task UpdateBrandAllAgentGameTypeSwitchAsync(BrandAllAgentGameType brandAllAgent)
        => UpdateEntityAsync(brandAllAgent);

    public Task<BrandAllAgentGameType> GetBrandAllAgentGameTypeAsync(
        Expression<Func<BrandAllAgentGameType, bool>> predicate,
        Func<IQueryable<BrandAllAgentGameType>, IQueryable<BrandAllAgentGameType>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);

    public Task DeleteBrandAllAgentGameTypeAsync(BrandAllAgentGameType brandAllAgent)
        => DeleteEntityAsync(brandAllAgent);
}