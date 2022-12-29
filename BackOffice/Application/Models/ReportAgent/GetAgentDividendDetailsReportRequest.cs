using System;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportAgent;

public class GetAgentDividendDetailsReportRequest : PageableRequest
{
    public Guid AgentId { get; set; }

    public decimal AgentDividendPercentage { get; init; }

    public DateTimeOffset From { get; set; }

    public DateTimeOffset To { get; set; }
}