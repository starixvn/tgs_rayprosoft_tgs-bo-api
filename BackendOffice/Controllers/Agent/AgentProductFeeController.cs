using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using TGS.Apis.BackendOffice.Controllers.Messages.AgentProductFee;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Agent;

[Route("api/[controller]")]
[ApiController]
public class AgentProductFeeController(IAgentClient agentClient) : BaseContextController
{
    [HttpGet("{agentId:guid}")]
    public async Task<GetAgentProductFeeResponse> GetAgentProductFeeAsync(Guid agentId)
    {
        var response = await agentClient.GetAgentProductFeeAsync(agentId, HttpContext.RequestAborted);

        return response == null
            ? new GetAgentProductFeeResponse { AgentId = agentId }
            : new GetAgentProductFeeResponse
            {
                AgentId = response.AgentId,
                CurrentCardGamePercentage = response.CurrentCardGamePercentage,
                CurrentLiveDealerPercentage = response.CurrentLiveDealerPercentage,
                CurrentLotteryPercentage = response.CurrentLotteryPercentage,
                CurrentSlotPercentage = response.CurrentSlotPercentage,
                CurrentSportsPercentage = response.CurrentSportsPercentage,
                CurrentVirtualSportsPercentage = response.CurrentVirtualSportsPercentage,
                NextCardGamePercentage = response.NextCardGamePercentage,
                NextLiveDealerPercentage = response.NextLiveDealerPercentage,
                NextLotteryPercentage = response.NextLotteryPercentage,
                NextSlotPercentage = response.NextSlotPercentage,
                NextSportsPercentage = response.NextSportsPercentage,
                NextVirtualSportsPercentage = response.NextVirtualSportsPercentage,
            };
    }

    [HttpPut(Name = "UpdateAgentProductFee")]
    public async ValueTask<MessageResponse> UpdateAgentProductFeeAsync(
        [FromBody] UpdateAgentProductFeeRequest request)
    {
        await agentClient.SetAgentProductFeeAsync(
            new SetAgentProductFeeRequest
            {
                AgentId = request.AgentId,
                CurrentCardGamePercentage = request.CurrentCardGamePercentage,
                CurrentLiveDealerPercentage = request.CurrentLiveDealerPercentage,
                CurrentLotteryPercentage = request.CurrentLotteryPercentage,
                CurrentSlotPercentage = request.CurrentSlotPercentage,
                CurrentSportsPercentage = request.CurrentSportsPercentage,
                CurrentVirtualSportsPercentage = request.CurrentVirtualSportsPercentage,
                NextCardGamePercentage = request.NextCardGamePercentage,
                NextLiveDealerPercentage = request.NextLiveDealerPercentage,
                NextLotteryPercentage = request.NextLotteryPercentage,
                NextSlotPercentage = request.NextSlotPercentage,
                NextSportsPercentage = request.NextSportsPercentage,
                NextVirtualSportsPercentage = request.NextVirtualSportsPercentage,
                EditorUsername = Username
            },
            HttpContext.RequestAborted);

        return CreateValidResponse<MessageResponse>();
    }
}
