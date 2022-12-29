using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Agent;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.AgentHandlers;

public class CreateAgentBankHandler : Handler<CreateAgentBankRequest, MessageResponse>
{
    private readonly IAgentBankService _service;

    public CreateAgentBankHandler(IAgentBankService service, IHttpContextAccessor accessor) : base(accessor)
    {
        _service = service;
    }

    public override async Task<MessageResponse> Handle(CreateAgentBankRequest request,
        CancellationToken cancellationToken)
    {
        await _service.CreateAgentBankAsync(request);

        return CreateValidResponse<MessageResponse>();
    }
}
