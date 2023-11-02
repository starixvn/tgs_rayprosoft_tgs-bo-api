using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class SetAgentPasswordCommandService : IGetService<SetAgentPasswordCommand, SetAgentPasswordResult>
{
    private readonly IAgentClient _agentClient;

    public SetAgentPasswordCommandService(
        IAgentClient agentClient)
    {
        _agentClient = agentClient ?? throw new ArgumentNullException(nameof(agentClient));
    }

    public async ValueTask<SetAgentPasswordResult> GetAsync(SetAgentPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var agent = await _agentClient
            .GetAgentInfoAsync(new GetAgentInfoRequest(command.AgentId), cancellationToken)
            .ConfigureAwait(false);

        if (agent is null)
            return new SetAgentPasswordResult(false, Application.Resources.Agent.NotFound);

        await _agentClient
            .SetAgentPasswordAsync(new SetAgentPasswordRequest(command.AgentId, command.Password, command.ModifyUserId), cancellationToken)
            .ConfigureAwait(false);

        await _agentClient
            .AddAgentAuditLogAsync(new AddAgentAuditLogRequest(command.AgentId, Tgs.Agent.Client.Models.Enums.AgentOperation.Password, command.ModifyUsername), cancellationToken)
            .ConfigureAwait(false);

        return new SetAgentPasswordResult(true, string.Empty);
    }
}
