# nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.Agent.Client.Models.Enums;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class SetAgentContactCommandService(
    IAgentClient agentClient) : IGetService<SetAgentContactCommand, SetAgentContactResult>
{
    private readonly IAgentClient _agentClient = agentClient ?? throw new ArgumentNullException(nameof(agentClient));

    public async ValueTask<SetAgentContactResult> GetAsync(SetAgentContactCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.ModifyContact.Any())
            return new SetAgentContactResult(false, string.Empty);

        var agent = await _agentClient
            .GetAgentInfoAsync(new GetAgentInfoRequest(command.AgentId), cancellationToken)
            .ConfigureAwait(false);

        if (agent is null)
            return new SetAgentContactResult(false, Application.Resources.Agent.NotFound);

        var modifyDict = new Dictionary<AgentPropertyName, string?>();

        var availablePropertyNames = new HashSet<string>(
            new string[]
            {
                nameof(AgentPropertyName.Phone),
                nameof(AgentPropertyName.Email),
                nameof(AgentPropertyName.Qq),
                nameof(AgentPropertyName.Wechat),
            },
            StringComparer.OrdinalIgnoreCase);

        foreach (var item in command.ModifyContact)
        {
            if (availablePropertyNames.Contains(item.Key))
            {
                modifyDict.Add(Enum.Parse<AgentPropertyName>(item.Key, true), item.Value);
            }
        }

        if (modifyDict.Count == 0)
            return new SetAgentContactResult(false, string.Empty);

        await _agentClient
            .SetAgentPropertiesAsync(command.AgentId, modifyDict.ToImmutableDictionary(), command.ModifyUserId, cancellationToken)
            .ConfigureAwait(false);

        await _agentClient.AddAgentAuditLogAsync(new AddAgentAuditLogRequest(command.AgentId, AgentOperation.Contact, command.ModifyUsername), cancellationToken)
            .ConfigureAwait(false);

        return new SetAgentContactResult(true, string.Empty);
    }
}
