using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class AllAgentsByBrandQueryService(IAgentRepository agentRepository) : IQueryService<AllAgentsByBrandQuery, AgentInfo>
{
    private readonly IAgentRepository _agentRepository = agentRepository ?? throw new ArgumentNullException(nameof(agentRepository));

    public IAsyncEnumerable<AgentInfo> QueryAsync(AllAgentsByBrandQuery query, CancellationToken cancellationToken = default)
        => _agentRepository.GetAgentsAsync(query.BrandId);
}
