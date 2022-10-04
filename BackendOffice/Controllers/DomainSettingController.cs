using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DomainSettingController : BaseContextController
{
    [HttpPut("ingress/{agentId}", Name = "ApplyAgentDomainIngress")]
    public ValueTask ApplyAgentDomainIngressAsync(
        [FromServices] ICommandService<ApplyAgentDomainIngressCommand> applyService,
        Guid agentId)
    {
        var command = new ApplyAgentDomainIngressCommand(agentId);

        return applyService.ExecuteAsync(command);
    }

    [HttpPut("dns-zone/{agentId}", Name = "CreateZoneByAgentId")]
    public ValueTask CreateDnsZoneAsync(
        [FromServices] ICommandService<CreateAgentDnsZoneCommand> createService,
        Guid agentId)
    {
        var command = new CreateAgentDnsZoneCommand(agentId);

        return createService.ExecuteAsync(command);
    }

    [HttpDelete("ingress/{agentId}", Name = "DeleteAgentDomainIngress")]
    public ValueTask DeleteAgentDomainIngressAsync(
        [FromServices] ICommandService<DeleteAgentDomainIngressCommand> deleteService,
        Guid agentId)
    {
        var command = new DeleteAgentDomainIngressCommand(agentId);

        return deleteService.ExecuteAsync(command);
    }

    [HttpDelete("dns-zone/{agentId}", Name = "DeleteZoneByAgentId")]
    public ValueTask DeleteDnsZoneAsync(
        [FromServices] ICommandService<DeleteAgentDnsZoneCommand> deleteService,
        Guid agentId)
    {
        var command = new DeleteAgentDnsZoneCommand(agentId);

        return deleteService.ExecuteAsync(command);
    }
}
