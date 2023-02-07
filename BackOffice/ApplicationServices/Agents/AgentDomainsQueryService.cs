using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class AgentDomainsQueryService : IQueryService<AgentDomainsQuery, string>
{
    private readonly IAgentRepository _agentRepository;

    public AgentDomainsQueryService(IAgentRepository agentRepository)
    {
        _agentRepository = agentRepository ?? throw new ArgumentNullException(nameof(agentRepository));
    }

    public IAsyncEnumerable<string> QueryAsync(AgentDomainsQuery query, CancellationToken cancellationToken = default)
        => _agentRepository.QueryDomainsFromAgentAsync(query.AgentId);
}
