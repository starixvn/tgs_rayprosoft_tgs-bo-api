using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class AgentHistoriesQueryService : IQueryService<AgentHistoriesQuery, AgentHistoryInfo>, IGetService<AgentHistoriesQuery, int>
{
    private readonly IAgentClient _agentClient;

    public AgentHistoriesQueryService(IAgentClient agentClient)
    {
        _agentClient = agentClient ?? throw new System.ArgumentNullException(nameof(agentClient));
    }

    public ValueTask<int> GetAsync(AgentHistoriesQuery query, CancellationToken cancellationToken = default)
        => _agentClient.CountAgentHistoriesAsync(
            new GetAgentHistoriesQueryConditions(query.AgentId, query.From, query.To));

    public IAsyncEnumerable<AgentHistoryInfo> QueryAsync(AgentHistoriesQuery query, CancellationToken cancellationToken = default)
        => AsyncEnumerableEx.Return(new GetAgentHistoriesQueryConditions(query.AgentId, query.From, query.To))
            .SelectMany(condition => _agentClient.CountAgentHistoriesAsync(
                condition,
                cancellationToken)
                .AsTask()
                .ToAsyncEnumerable()
                .SelectMany(count => _agentClient.GetAgentHistoriesAsync(
                    condition,
                    count,
                    query.Skip,
                    cancellationToken)));
}
