using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class GetAgentOperationsRequest : PageableRequest, IRequest<PagedResponse<AgentOperationModel>>
{
    public GetAgentOperationsRequest(Guid id)
    {
        AgentId = id;
    }

    public Guid AgentId { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
}