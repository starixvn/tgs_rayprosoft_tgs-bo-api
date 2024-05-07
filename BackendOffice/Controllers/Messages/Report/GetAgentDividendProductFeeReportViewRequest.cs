using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Report;

public class GetAgentDividendProductFeeReportViewRequest
{
    public Guid AgentId { get; init; }
    public decimal AgentDividendPercentage { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}
