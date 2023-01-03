using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class GetAgentOperationsRequest : PageableRequest, IRequest<PagedResponse<AgentOperationModel>>
{
    public Guid AgentId { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public GetAgentOperationsRequest(Guid id)
    {
        AgentId = id;
    }
}
