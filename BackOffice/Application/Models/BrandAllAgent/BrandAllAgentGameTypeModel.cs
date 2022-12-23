using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.BrandAllAgent;

public record BrandAllAgentGameTypeModel
{
    public bool Switch { get; set; }

    public bool HasValidBet { get; set; }

    public bool HasWinLoss { get; set; }

    public AllAgentGameType AllAgentGameType { get; set; }
}
