using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.DomainSettings.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class CreateAgentDnsZoneCommandService : ICommandService<CreateAgentDnsZoneCommand>
{
    private readonly IDomainSettingClient _domainSettingClient;

    public CreateAgentDnsZoneCommandService(IDomainSettingClient domainSettingClient)
    {
        _domainSettingClient = domainSettingClient ?? throw new ArgumentNullException(nameof(domainSettingClient));
    }

    public ValueTask ExecuteAsync(CreateAgentDnsZoneCommand command, CancellationToken cancellationToken = default)
        => _domainSettingClient.CreateZoneAsync(command.AgentId);
}
