using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Agent;

namespace TGS.Apis.BackendOffice.Controllers.Agent;

public partial class AgentController
{
    [HttpPost("{agentId}/bank")]
    [Description("Create an Agent bank")]
    public Task<MessageResponse> CreateAgentBankAsync(Guid agentId, [FromBody] CreateAgentBankRequest request)
    {
        request.AgentId = agentId;
        return _mediator.Send(request);
    }

    [HttpPut("bank/{id}")]
    [Description("Update an Agent bank")]
    public Task<MessageResponse> UpdateAgentBankAsync(Guid id, [FromBody] UpdateAgentBankRequest request)
    {
        request.Id = id;
        return _mediator.Send(request);
    }

    [HttpDelete("bank/{id}")]
    [Description("Delete an Agent bank")]
    public Task<MessageResponse> DeleteAgentBankAsync(Guid id) =>
        _mediator.Send(new DeleteAgentBankRequest(id));
}