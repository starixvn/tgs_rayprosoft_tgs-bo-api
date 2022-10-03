using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Services;

internal class RemoveAgentDomainCommandService : ICommandService<RemoveAgentDomainCommand>
{
    private readonly IAgentRepository _agentRepository;

    public RemoveAgentDomainCommandService(IAgentRepository agentRepository)
    {
        _agentRepository = agentRepository ?? throw new ArgumentNullException(nameof(agentRepository));
    }

    public ValueTask ExecuteAsync(RemoveAgentDomainCommand command, CancellationToken cancellationToken = default)
        => _agentRepository.RemoveDomainFromAgentAsync(command.AgentId, command.DomainName);
}
