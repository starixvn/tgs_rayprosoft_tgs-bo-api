using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Agent.Dividend.Client;
using Tgs.Agent.Dividend.Client.Models;
using TGS.Apis.BackendOffice.Controllers.Messages.AgentContract;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Agent;

[Route("api/[controller]")]
[ApiController]
public class AgentContractController : BaseContextController
{
    [HttpGet]
    public async Task<PagedResponse<AgentDividendContract>> GetAgentContractsAsync(
        [FromServices] IAgentContractClient agentContractClient,
        [FromQuery] GetAgentContractsRequest request)
    {
        var list = await agentContractClient.GetAgentContractAsync(request.AgentId, HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted);

        return list.AsQueryable().CreatePagedResponse(request);
    }
}
