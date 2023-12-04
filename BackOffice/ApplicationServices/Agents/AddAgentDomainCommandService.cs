using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class AddAgentDomainCommandService(IAgentRepository agentRepository) : ICommandService<AddAgentDomainCommand>
{
    private readonly IAgentRepository _agentRepository = agentRepository ?? throw new ArgumentNullException(nameof(agentRepository));

    public ValueTask ExecuteAsync(
        AddAgentDomainCommand command,
        CancellationToken cancellationToken = default)
        => _agentRepository.AddDomainToAgentAsync(command.AgentId, command.DomainName);
}
