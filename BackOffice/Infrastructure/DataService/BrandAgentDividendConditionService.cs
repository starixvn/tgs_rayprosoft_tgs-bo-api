using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IBrandAgentDividendConditionService
{
    Task CreateBrandAgentDividendConditionsAsync(BrandAgentDividendCondition[] brandAgentDividendConditions);

    IQueryable<BrandAgentDividendCondition> GetBrandAgentDividendConditionQueries(
            Func<IQueryable<BrandAgentDividendCondition>, IQueryable<BrandAgentDividendCondition>> preQuery = null,
        bool tracking = false);

    Task UpdateBrandAgentDividendConditionsAsync(Guid brandId,
        BrandAgentDividendCondition[] brandAgentDividendConditions);
}

public class BrandAgentDividendConditionService : BaseRepositoryOperations<ITgsRepository>,
    IBrandAgentDividendConditionService
{
    private readonly ITgsRepository _repository;

    public BrandAgentDividendConditionService(ITgsRepository tgsRepository) : base(tgsRepository)
        => _repository = tgsRepository;

    public Task CreateBrandAgentDividendConditionsAsync(BrandAgentDividendCondition[] brandAgentDividendConditions)
        => CreateEntitiesAsync(brandAgentDividendConditions);

    public IQueryable<BrandAgentDividendCondition> GetBrandAgentDividendConditionQueries(
        Func<IQueryable<BrandAgentDividendCondition>, IQueryable<BrandAgentDividendCondition>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);

    public async Task UpdateBrandAgentDividendConditionsAsync(Guid brandId,
        BrandAgentDividendCondition[] brandAgentDividendConditions)
    {
        var currentBrandAgentDividendConditions = await _repository.BrandAgentDividendConditions.AsQueryable()
            .Where(x => x.BrandId == brandId).ToArrayAsync().ConfigureAwait(false);

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        await DeleteEntitiesAsync(currentBrandAgentDividendConditions);
        await CreateEntitiesAsync(brandAgentDividendConditions);

        scope.Complete();
    }
}
