using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class AddAgentDomainCommandService : ICommandService<AddAgentDomainCommand>
{
    private readonly IAgentRepository _agentRepository;

    public AddAgentDomainCommandService(IAgentRepository agentRepository)
    {
        _agentRepository = agentRepository ?? throw new ArgumentNullException(nameof(agentRepository));
    }

    public ValueTask ExecuteAsync(
        AddAgentDomainCommand command,
        CancellationToken cancellationToken = default)
        => _agentRepository.AddDomainToAgentAsync(command.AgentId, command.DomainName);
}
