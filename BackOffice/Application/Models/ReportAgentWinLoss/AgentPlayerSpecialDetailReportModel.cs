namespace TGS.Services.BackOffice.Application.Models.ReportAgentWinLoss;

public record AgentPlayerSpecialDetailReportModel
{
    public decimal Bonus { get; set; }
    public decimal BonusAdjust { get; set; }
    public decimal CompanyAdjust { get; set; }
    public decimal AgentAdjust { get; set; }
}