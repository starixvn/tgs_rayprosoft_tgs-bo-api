using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tgs.Player.All.Agent.Client;
using Tgs.Player.All.Agent.Client.Models;
using TGS.Services.BackOffice.Application.Models.BrandAllAgent;
using TGS.Services.BackOffice.Domain;
using ValidBetConditionModel = Tgs.Player.All.Agent.Client.Models.ValidBetConditionModel;

namespace TGS.Services.BackOffice.Application.Commands.BrandAllAgentHandlers;

public class UpdateBrandAllAgentValidBetConditionHandler(
    IBrandAllAgentClient client,
    IHttpContextAccessor accessor)
    : Handler<UpdateBrandAllAgentValidBetConditionRequest, MessageResponse>(accessor)
{
    private readonly IBrandAllAgentClient _client = client;

    public override async Task<MessageResponse> Handle(UpdateBrandAllAgentValidBetConditionRequest request,
        CancellationToken cancellationToken)
    {
        var conditions = request.Conditions.Select(x => new ValidBetConditionModel
        {
            Rank = x.Rank,
            ActiveNumber = x.ActiveNumber,
            NetProfit = x.NetProfit,
            RebatePercentage = x.RebatePercentage
        })
            .ToArray();

        await _client.UpdateValidBetConditionAsync(
            new UpdateValidBetConditionRequest(request.BrandId,
                conditions,
                (AllAgentGameType)request.AllAgentGameType));

        return CreateValidResponse<MessageResponse>();
    }
}
