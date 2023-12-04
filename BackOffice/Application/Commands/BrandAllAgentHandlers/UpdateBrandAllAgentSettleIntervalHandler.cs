using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using Tgs.Player.All.Agent.Client;
using Tgs.Player.All.Agent.Client.Models;
using TGS.Services.BackOffice.Application.Models.BrandAllAgent;

namespace TGS.Services.BackOffice.Application.Commands.BrandAllAgentHandlers;

public class
    UpdateBrandAllAgentSettleIntervalHandler(
    IBrandAllAgentClient client,
    IHttpContextAccessor accessor) : Handler<UpdateBrandAllAgentSettleIntervalRequest, MessageResponse>(accessor)
{
    private readonly IBrandAllAgentClient _client = client;

    public override async Task<MessageResponse> Handle(UpdateBrandAllAgentSettleIntervalRequest request,
        CancellationToken cancellationToken)
    {
        await _client.UpdateSettlementIntervalAsync(new UpdateSettlementIntervalRequest(request.BrandId,
            (SettlementIntervalType)request.SettlementIntervalType));

        return CreateValidResponse<MessageResponse>();
    }
}