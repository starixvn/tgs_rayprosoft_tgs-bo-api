using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.AgentAdjust;

public class GetAdjustAgentsRequest : PageableRequest, IRequest<PagedResponse<AdjustAgentModel>>
{
    public Guid BrandId { get; set; }
    public AgentStatus? Status { get; set; }
    public AgentType? Type { get; set; }
    public AgentLevelType? LevelType { get; set; }
    public string Username { get; set; }
}