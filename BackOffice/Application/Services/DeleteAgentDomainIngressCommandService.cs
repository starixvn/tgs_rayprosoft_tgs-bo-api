using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.DomainSettings.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Services;

internal class DeleteAgentDomainIngressCommandService : ICommandService<DeleteAgentDomainIngressCommand>
{
    private readonly IDomainSettingClient _domainSettingClient;

    public DeleteAgentDomainIngressCommandService(IDomainSettingClient domainSettingClient)
    {
        _domainSettingClient = domainSettingClient ?? throw new ArgumentNullException(nameof(domainSettingClient));
    }

    public ValueTask ExecuteAsync(DeleteAgentDomainIngressCommand command, CancellationToken cancellationToken = default)
        => _domainSettingClient.DeleteAgentDomainIngressAsync(command.AgentId);
}
