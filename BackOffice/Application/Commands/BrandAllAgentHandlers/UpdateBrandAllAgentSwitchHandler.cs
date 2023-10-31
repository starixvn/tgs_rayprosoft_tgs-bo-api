using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using Tgs.Player.All.Agent.Client;
using Tgs.Player.All.Agent.Client.Models;
using TGS.Services.BackOffice.Application.Models.BrandAllAgent;

namespace TGS.Services.BackOffice.Application.Commands.BrandAllAgentHandlers;

public class UpdateBrandAllAgentSwitchHandler : Handler<UpdateBrandAllAgentSwitchRequest, MessageResponse>
{
    private readonly IBrandAllAgentClient _client;

    public UpdateBrandAllAgentSwitchHandler(
        IBrandAllAgentClient client,
        IHttpContextAccessor accessor)
        : base(accessor) =>
        _client = client;

    public override async Task<MessageResponse> Handle(UpdateBrandAllAgentSwitchRequest request,
        CancellationToken cancellationToken)
    {
        await _client.UpdateSwitchAsync(new UpdateSwitchRequest(request.BrandId, request.Switch));

        return CreateValidResponse<MessageResponse>();
    }
}