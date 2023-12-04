using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.DomainSettings.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class DeleteAgentDnsZoneCommandService(IDomainSettingClient domainSettingClient) : ICommandService<DeleteAgentDnsZoneCommand>
{
    private readonly IDomainSettingClient _domainSettingClient = domainSettingClient ?? throw new ArgumentNullException(nameof(domainSettingClient));

    public ValueTask ExecuteAsync(DeleteAgentDnsZoneCommand command, CancellationToken cancellationToken = default)
        => _domainSettingClient.DeleteZoneAsync(command.AgentId);
}
