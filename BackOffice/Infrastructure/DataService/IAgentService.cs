using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.BackOffice.Application.Models.Agent;
using CreateAgentRequest = TGS.Services.BackOffice.Application.Models.Agent.CreateAgentRequest;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IAgentService
{
    Task CreateAgentAsync(CreateAgentRequest request, Agent parentAgent = null);

    Task<bool> ExistsAsync(Expression<Func<Agent, bool>> predicate);

    Task<Agent> GetAgentAsync(
        Expression<Func<Agent, bool>> predicate,
        Func<IQueryable<Agent>, IQueryable<Agent>> preQuery = null);

    IQueryable<AgentAuditLog> GetAgentAuditLogQueries(
        Func<IQueryable<AgentAuditLog>, IQueryable<AgentAuditLog>> preQuery = null,
        bool tracking = false);

    IQueryable<Agent> GetAgentQueries(
        Func<IQueryable<Agent>, IQueryable<Agent>> preQuery = null,
        bool tracking = false);

    Task<(IEnumerable<AgentsModel>, int)> GetAgentsAsync(GetAgentsRequest request);

    Task<IEnumerable<AgentPathModel>> GetSubAgentsAsync(Guid agentId);
}
