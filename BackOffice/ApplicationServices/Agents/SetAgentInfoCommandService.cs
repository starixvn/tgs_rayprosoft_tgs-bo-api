#nullable enable

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

internal class SetAgentInfoCommandService : IGetService<SetAgentInfoCommand, SetAgentInfoResult>
{
    private readonly IAgentClient _agentClient;

    public SetAgentInfoCommandService(IAgentClient agentClient)
    {
        _agentClient = agentClient ?? throw new ArgumentNullException(nameof(agentClient));
    }

    public async ValueTask<SetAgentInfoResult> GetAsync(SetAgentInfoCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.ModifyInfo.Any())
            return new SetAgentInfoResult(false, string.Empty);

        var agent = await _agentClient
            .GetAgentInfoAsync(new GetAgentInfoRequest(command.AgentId), cancellationToken)
            .ConfigureAwait(false);

        if (agent is null)
            return new SetAgentInfoResult(false, Application.Resources.Agent.NotFound);

        var availablePropertyNames = new HashSet<string>(
            new string[]
            {
                nameof(AgentPropertyName.RealName),
                nameof(AgentPropertyName.Remark),
            },
            StringComparer.OrdinalIgnoreCase);

        var modifyDict = new Dictionary<AgentPropertyName, string?>();

        foreach (var item in command.ModifyInfo)
        {
            if (availablePropertyNames.Contains(item.Key))
            {
                modifyDict.Add(Enum.Parse<AgentPropertyName>(item.Key, true), item.Value);
            }
        }

        await _agentClient
            .SetAgentPropertiesAsync(command.AgentId, modifyDict.ToImmutableDictionary(), command.ModifyUserId, cancellationToken)
            .ConfigureAwait(false);

        await _agentClient.AddAgentAuditLogAsync(
            new AddAgentAuditLogRequest(command.AgentId, AgentOperation.Info, command.ModifyUsername),
            cancellationToken)
            .ConfigureAwait(false);

        return new SetAgentInfoResult(true, string.Empty);
    }
}
