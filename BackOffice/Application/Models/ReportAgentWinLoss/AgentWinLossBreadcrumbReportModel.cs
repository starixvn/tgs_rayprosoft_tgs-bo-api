namespace TGS.Services.BackOffice.Application.Models.ReportAgentWinLoss;

public record AgentWinLossBreadcrumbReportModel
{
    public string Name { get; set; } = default!;
    public string Id { get; set; } = default!;
    public string MemberType { get; set; } = default!;
}