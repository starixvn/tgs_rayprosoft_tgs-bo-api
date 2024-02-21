using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Services.BackOffice.Application.Models.Agent;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.AgentHandlers;

public class GetAgentsQueryHandler(
    IAgentService service,
    IHttpContextAccessor accessor)
    : Handler<GetAgentsRequest, PagedResponse<AgentsModel>>(accessor)
{
    public override async Task<PagedResponse<AgentsModel>> Handle(GetAgentsRequest request,
        CancellationToken cancellationToken)
    {
        var (agents, totalCount) = await service.GetAgentsAsync(request);

        return await agents.CreatePagedResponseAsync(totalCount);
    }
}
