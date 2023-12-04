using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using Tgs.Player.All.Agent.Client;
using Tgs.Player.All.Agent.Client.Models;
using TGS.Services.BackOffice.Application.Models.BrandAllAgent;

namespace TGS.Services.BackOffice.Application.Commands.BrandAllAgentHandlers;

public class
    UpdateBrandAllAgentGameTypeGameTypeSwitchHandler(
    IBrandAllAgentClient client,
    IHttpContextAccessor accessor) : Handler<UpdateBrandAllAgentGameTypeRequest, MessageResponse>(accessor)
{
    private readonly IBrandAllAgentClient _client = client;

    public override async Task<MessageResponse> Handle(UpdateBrandAllAgentGameTypeRequest request,
        CancellationToken cancellationToken)
    {
        var gameTypeSwitch = new UpdateGameTypeSwitchRequest(request.BrandId,
            request.Switch,
            request.HasValidBet,
            (AllAgentGameType)request.AllAgentGameType);

        await _client.UpdateGameTypeSwitchAsync(gameTypeSwitch);

        return CreateValidResponse<MessageResponse>();
    }
}