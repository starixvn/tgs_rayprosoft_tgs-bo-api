using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Agent.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class AgentHistoryQueryService : IGetService<AgentHistoryQuery, AgentHistoryEventInfo>
{
    private readonly IAgentClient _agentClient;

    public AgentHistoryQueryService(IAgentClient agentClient)
    {
        _agentClient = agentClient ?? throw new ArgumentNullException(nameof(agentClient));
    }

    public async ValueTask<AgentHistoryEventInfo> GetAsync(AgentHistoryQuery query, CancellationToken cancellationToken = default)
    {
        var info = await _agentClient
            .GetAgentHistoryAsync(query.EventId, cancellationToken)
            .ConfigureAwait(false);

        return new AgentHistoryEventInfo(info.Action, info.ChangeSets, info.CreatedOn, info.Modifier);
    }
}
