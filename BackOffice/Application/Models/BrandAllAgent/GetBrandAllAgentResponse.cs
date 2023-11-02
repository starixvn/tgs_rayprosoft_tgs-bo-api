using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.BrandAllAgent;

public record GetBrandAllAgentResponse
{
    public bool Switch { get; set; }
    public SettlementIntervalType? SettlementIntervalType { get; set; }
    public BrandAllAgentGameTypeModel[] BrandAllAgentGameTypes { get; set; }
}
