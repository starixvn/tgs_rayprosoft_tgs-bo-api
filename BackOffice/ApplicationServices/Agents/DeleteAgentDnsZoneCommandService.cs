using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.DomainSettings.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class DeleteAgentDnsZoneCommandService : ICommandService<DeleteAgentDnsZoneCommand>
{
    private readonly IDomainSettingClient _domainSettingClient;

    public DeleteAgentDnsZoneCommandService(IDomainSettingClient domainSettingClient)
    {
        _domainSettingClient = domainSettingClient ?? throw new ArgumentNullException(nameof(domainSettingClient));
    }

    public ValueTask ExecuteAsync(DeleteAgentDnsZoneCommand command, CancellationToken cancellationToken = default)
        => _domainSettingClient.DeleteZoneAsync(command.AgentId);
}
