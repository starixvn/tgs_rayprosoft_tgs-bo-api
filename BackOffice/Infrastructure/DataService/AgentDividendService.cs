using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IAgentDividendService
{
    Task<AgentDividend> GetAgentDividendAsync(Expression<Func<AgentDividend, bool>> predicate,
        Func<IQueryable<AgentDividend>, IQueryable<AgentDividend>> preQuery = null);

    IQueryable<AgentDividend> GetAgentDividends(
        Func<IQueryable<AgentDividend>, IQueryable<AgentDividend>> preQuery = null,
        bool tracking = false);
}

public class AgentDividendService : BaseRepositoryOperations<ITgsRepository>, IAgentDividendService
{
    private readonly ITgsRepository _repository;

    public AgentDividendService(ITgsRepository repository)
        : base(repository)
    {
        _repository = repository;
    }

    Task<AgentDividend> IAgentDividendService.GetAgentDividendAsync(Expression<Func<AgentDividend, bool>> predicate,
        Func<IQueryable<AgentDividend>, IQueryable<AgentDividend>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);

    IQueryable<AgentDividend> IAgentDividendService.GetAgentDividends(
        Func<IQueryable<AgentDividend>, IQueryable<AgentDividend>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);
}