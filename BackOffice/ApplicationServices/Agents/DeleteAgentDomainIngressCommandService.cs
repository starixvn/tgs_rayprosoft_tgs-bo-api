using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.DomainSettings.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class DeleteAgentDomainIngressCommandService(IDomainSettingClient domainSettingClient) : ICommandService<DeleteAgentDomainIngressCommand>
{
    private readonly IDomainSettingClient _domainSettingClient = domainSettingClient ?? throw new ArgumentNullException(nameof(domainSettingClient));

    public ValueTask ExecuteAsync(DeleteAgentDomainIngressCommand command, CancellationToken cancellationToken = default)
        => _domainSettingClient.DeleteAgentDomainIngressAsync(command.AgentId);
}
