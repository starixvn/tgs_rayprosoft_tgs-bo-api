using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class GetAgentRealNameQueryService : IGetService<GetAgentRealNameQuery, string>
{
    private readonly IAgentService _agentService;

    public GetAgentRealNameQueryService(IAgentService agentService)
    {
        _agentService = agentService ?? throw new ArgumentNullException(nameof(agentService));
    }

    public async ValueTask<string> GetAsync(GetAgentRealNameQuery query, CancellationToken cancellationToken = default)
    {
        var agent = await _agentService
            .GetAgentAsync(agent => agent.Id == query.AgentId)
            .ConfigureAwait(false);

        return agent.RealName ?? string.Empty;
    }
}
