namespace TGS.Services.BackOffice.Application.Models.ReportAgentWinLoss;

public record AgentPlayerSpecialDetailReportModel
{
    public decimal Bonus { get; set; }
    public decimal BonusAdjust { get; set; }
    public decimal PlatformCharge { get; set; }
    public decimal ProductFee { get; set; }
}