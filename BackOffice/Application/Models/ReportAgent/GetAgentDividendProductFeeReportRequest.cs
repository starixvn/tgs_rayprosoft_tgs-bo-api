using System;

namespace TGS.Services.BackOffice.Application.Models.ReportAgent;

public class GetAgentDividendProductFeeReportRequest
{
    public Guid AgentId { get; init; }
    public decimal AgentDividendPercentage { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}
