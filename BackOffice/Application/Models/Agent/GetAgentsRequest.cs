using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class GetAgentsRequest : PageableRequest, IRequest<PagedResponse<AgentModel>>
{
    public Guid BrandId { get; set; }

    public AgentLevelType? LevelType { get; set; }

    public AgentStatus? Status { get; set; }

    public AgentType? Type { get; set; }

    public string Username { get; set; }
}
