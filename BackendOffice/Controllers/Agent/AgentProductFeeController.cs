using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using TGS.Apis.BackendOffice.Controllers.Messages.AgentProductFee;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Agent
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgentProductFeeController(IAgentClient agentClient) : BaseContextController
    {
        [HttpGet("{agentId:guid}")]
        public async Task<GetAgentProductFeeResponse> GetAgentProductFeeAsync(Guid agentId)
        {
            var response = await agentClient.GetAgentProductFeeAsync(agentId, HttpContext.RequestAborted);

            return response == null
                ? new GetAgentProductFeeResponse{AgentId = agentId }
                : new GetAgentProductFeeResponse
                {
                    AgentId = response.AgentId,
                    CardGamePercentage = response.CardGamePercentage,
                    LiveDealerPercentage = response.LiveDealerPercentage,
                    LotteryPercentage = response.LotteryPercentage,
                    SlotPercentage = response.SlotPercentage,
                    SportsPercentage = response.SportsPercentage,
                    VirtualSportsPercentage = response.VirtualSportsPercentage,
                };
        }

        [HttpPut("{agentId}")]
        public async ValueTask<MessageResponse> UpdateAgentProductFeeAsync(
            [FromBody] UpdateAgentProductFeeRequest request,
            Guid agentId)
        {
            await agentClient.SetAgentProductFeeAsync(new SetAgentProductFeeRequest
                {
                    AgentId = agentId,
                    CardGamePercentage = request.CardGamePercentage,
                    LiveDealerPercentage = request.LiveDealerPercentage,
                    LotteryPercentage = request.LotteryPercentage,
                    SlotPercentage = request.SlotPercentage,
                    SportsPercentage = request.SportsPercentage,
                    VirtualSportsPercentage = request.VirtualSportsPercentage,
                    EditorUsername = Username
                },
                HttpContext.RequestAborted);

            return CreateValidResponse<MessageResponse>();
        }
    }
}
