using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Agent;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.AgentHandlers;

public class DeleteAgentBankHandler(IAgentBankService service, IHttpContextAccessor accessor) : Handler<DeleteAgentBankRequest, MessageResponse>(accessor)
{
    private readonly IAgentBankService _service = service;

    public override async Task<MessageResponse> Handle(DeleteAgentBankRequest request,
        CancellationToken cancellationToken)
    {
        var bank = await _service.GetAgentBankAsync(q => q.Id == request.Id);

        if (bank == null)
        {
            return CreateInvalidResponse<MessageResponse>(AgentBank.NotFound);
        }

        await _service.DeleteAgentBankAsync(bank);

        return CreateValidResponse<MessageResponse>();
    }
}
