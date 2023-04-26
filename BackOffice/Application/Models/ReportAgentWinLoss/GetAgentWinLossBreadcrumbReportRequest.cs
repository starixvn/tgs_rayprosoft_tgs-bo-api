using System;

namespace TGS.Services.BackOffice.Application.Models.ReportAgentWinLoss;

public class GetAgentWinLossBreadcrumbReportRequest
{
    public Guid? AgentId { get; init; }
    public string PlayerCode { get; init; }
    public Guid? RoundId { get; init; }
}
