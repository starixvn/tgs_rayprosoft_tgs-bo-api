using System;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Report;

public class GetAgentPlayerWinLossReportViewRequest : PageableRequest
{
    public decimal AgentDividendPercentage { get; init; }

    public Guid AgentId { get; set; }

    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }
}
