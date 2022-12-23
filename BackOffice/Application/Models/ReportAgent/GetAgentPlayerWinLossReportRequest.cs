using System;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportAgent;

public class GetAgentPlayerWinLossReportRequest : PageableRequest
{
    public Guid AgentId { get; set; }

    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }
}
