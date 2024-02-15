using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Services.BackOffice.Application.Models.Agent;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.AgentHandlers;

public class UpdateAgentBankHandler(
    IAgentBankService service,
    IHttpContextAccessor accessor)
    : Handler<UpdateAgentBankRequest, MessageResponse>(accessor)
{
    private readonly IAgentBankService _service = service;

    public override async Task<MessageResponse> Handle(UpdateAgentBankRequest request,
        CancellationToken cancellationToken)
    {
        var agent = await _service.GetAgentBankAsync(q => q.Id == request.Id);

        if (agent is null)
        {
            return CreateInvalidResponse<MessageResponse>(AgentBank.NotFound);
        }

        await _service.UpdateAgentBankAsync(agent, request);

        return CreateValidResponse<MessageResponse>();
    }
}
