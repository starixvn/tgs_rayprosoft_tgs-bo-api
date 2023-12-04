using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.Agent.Client.Models.Enums;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class SetAgentStatusCommandService(IAgentClient agentClient) : IGetService<SetAgentStatusCommand, SetAgentStatusResult>
{
    private readonly IAgentClient _agentClient = agentClient ?? throw new ArgumentNullException(nameof(agentClient));

    public async ValueTask<SetAgentStatusResult> GetAsync(SetAgentStatusCommand command, CancellationToken cancellationToken = default)
    {
        var agent = await _agentClient
            .GetAgentInfoAsync(new GetAgentInfoRequest(command.AgentId), cancellationToken)
            .ConfigureAwait(false);

        if (agent is null)
            return new SetAgentStatusResult(false, Application.Resources.Agent.NotFound);

        await _agentClient
            .SetAgentStatusAsync(command.AgentId, command.Status, command.ModifyUserId, cancellationToken)
            .ConfigureAwait(false);

        await _agentClient
            .AddAgentAuditLogAsync(new AddAgentAuditLogRequest(command.AgentId, AgentOperation.Info, command.ModifyUsername), cancellationToken)
            .ConfigureAwait(false);

        return new SetAgentStatusResult(true, string.Empty);
    }
}
