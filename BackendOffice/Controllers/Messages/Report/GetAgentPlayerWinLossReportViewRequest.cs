using System;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Report;

public class GetAgentPlayerWinLossReportViewRequest : PageableRequest
{
    public decimal AgentDividendPercentage { get; init; }

    public Guid AgentId { get; set; }

    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }
}
