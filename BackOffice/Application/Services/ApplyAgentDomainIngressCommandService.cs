using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.DomainSettings.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Services;

internal class ApplyAgentDomainIngressCommandService : ICommandService<ApplyAgentDomainIngressCommand>
{
    private readonly IDomainSettingClient _domainSettingClient;

    public ApplyAgentDomainIngressCommandService(IDomainSettingClient domainSettingClient)
    {
        _domainSettingClient = domainSettingClient ?? throw new ArgumentNullException(nameof(domainSettingClient));
    }

    public ValueTask ExecuteAsync(ApplyAgentDomainIngressCommand command, CancellationToken cancellationToken = default)
        => _domainSettingClient.ApplyAgentDomainIngressAsync(command.AgentId);
}
