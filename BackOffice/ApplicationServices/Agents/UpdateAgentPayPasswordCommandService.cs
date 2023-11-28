using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.Agent.Client.Models.Enums;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class UpdateAgentPayPasswordCommandService : ICommandService<UpdateAgentPayPasswordCommand>
{
    private readonly IAgentClient _agentClient;

    public UpdateAgentPayPasswordCommandService(IAgentClient agentClient)
    {
        _agentClient = agentClient ?? throw new ArgumentNullException(nameof(agentClient));
    }

    public async ValueTask ExecuteAsync(UpdateAgentPayPasswordCommand command, CancellationToken cancellationToken = default)
    {
        await _agentClient
            .SetAgentPayPasswordAsync(new SetAgentPayPasswordRequest(command.AgentId, command.PayPassword, command.EditUserId), cancellationToken)
            .ConfigureAwait(false);

        await _agentClient
            .AddAgentAuditLogAsync(new AddAgentAuditLogRequest(command.AgentId, AgentOperation.PayPassword, command.EditUsername), cancellationToken)
            .ConfigureAwait(false);
    }
}
