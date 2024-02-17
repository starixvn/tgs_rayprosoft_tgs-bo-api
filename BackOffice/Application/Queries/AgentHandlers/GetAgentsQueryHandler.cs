using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.CashService.Client;
using TGS.Services.BackOffice.Application.Models.Agent;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.AgentHandlers;

public class GetAgentsQueryHandler(
    IAgentService service,
    ICashServiceClient cashServiceClient,
    IAgentClient agentClient,
    IHttpContextAccessor accessor)
    : Handler<GetAgentsRequest, PagedResponse<AgentModel>>(accessor)
{
    public override async Task<PagedResponse<AgentModel>> Handle(GetAgentsRequest request,
        CancellationToken cancellationToken)
    {
        var (agents, totalCount) = await service.GetAgentsAsync(request);

        var agentModels = agents.ToArray();

        foreach (var agentModel in agentModels)
        {
            var agent = await agentClient.GetAgentInfoAsync(new GetAgentInfoRequest(agentModel.Id), cancellationToken).ConfigureAwait(false);

            var balance = await cashServiceClient.GetBalanceAsync(agent!.WalletId!.Value, cancellationToken);

            agentModel.Balance = balance;
        }

        return await agentModels.CreatePagedResponseAsync(totalCount);
    }
}
