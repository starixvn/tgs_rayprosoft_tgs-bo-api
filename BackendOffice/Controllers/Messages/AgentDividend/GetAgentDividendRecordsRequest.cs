#nullable enable
using System;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Domain.Enums;

namespace TGS.Apis.BackendOffice.Controllers.Messages.AgentDividend;

public class GetAgentDividendRecordsRequest : PageableRequest
{
    public Guid BrandId { get; set; }
    public AgentDividendDistributeStatus? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public DateTime? ApprovedFrom { get; set; }
    public DateTime? ApprovedTo { get; set; }
    public string? AgentUsername { get; set; }
    public bool? NegativeAgentDividendCarryForward { get; set; }
}
