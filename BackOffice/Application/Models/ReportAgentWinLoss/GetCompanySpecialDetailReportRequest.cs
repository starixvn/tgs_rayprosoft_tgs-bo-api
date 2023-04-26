using System;

namespace TGS.Services.BackOffice.Application.Models.ReportAgentWinLoss;

public record GetCompanySpecialDetailReportRequest
{
    public Guid BrandId { get; init; }
    public Guid AgentId { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}