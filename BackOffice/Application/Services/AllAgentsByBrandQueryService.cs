using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Services;

internal class AllAgentsByBrandQueryService : IQueryService<AllAgentsByBrandQuery, AgentInfo>
{
    private readonly IAgentRepository _agentRepository;

    public AllAgentsByBrandQueryService(IAgentRepository agentRepository)
    {
        _agentRepository = agentRepository ?? throw new ArgumentNullException(nameof(agentRepository));
    }

    public IAsyncEnumerable<AgentInfo> QueryAsync(AllAgentsByBrandQuery query, CancellationToken cancellationToken = default)
        => _agentRepository.GetAgentsAsync(query.BrandId);
}
