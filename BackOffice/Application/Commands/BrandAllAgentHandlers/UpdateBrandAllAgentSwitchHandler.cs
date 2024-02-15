using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tgs.Player.All.Agent.Client;
using Tgs.Player.All.Agent.Client.Models;
using TGS.Services.BackOffice.Application.Models.BrandAllAgent;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Commands.BrandAllAgentHandlers;

public class UpdateBrandAllAgentSwitchHandler(
    IBrandAllAgentClient client,
    IHttpContextAccessor accessor) 
    : Handler<UpdateBrandAllAgentSwitchRequest, MessageResponse>(accessor)
{
    private readonly IBrandAllAgentClient _client = client;

    public override async Task<MessageResponse> Handle(UpdateBrandAllAgentSwitchRequest request,
        CancellationToken cancellationToken)
    {
        await _client.UpdateSwitchAsync(new UpdateSwitchRequest(request.BrandId, request.Switch));

        return CreateValidResponse<MessageResponse>();
    }
}