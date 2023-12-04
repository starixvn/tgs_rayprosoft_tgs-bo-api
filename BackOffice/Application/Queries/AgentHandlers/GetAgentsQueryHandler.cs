using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Agent;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.AgentHandlers;

public class GetAgentsQueryHandler(IAgentService service, IHttpContextAccessor accessor) : Handler<GetAgentsRequest, PagedResponse<AgentModel>>(accessor)
{
    private readonly IAgentService _service = service;

    public override async Task<PagedResponse<AgentModel>> Handle(GetAgentsRequest request,
        CancellationToken cancellationToken)
    {
        var (agents, totalCount) = await _service.GetAgentsAsync(request);

        return await agents.CreatePagedResponseAsync(totalCount);
    }
}
