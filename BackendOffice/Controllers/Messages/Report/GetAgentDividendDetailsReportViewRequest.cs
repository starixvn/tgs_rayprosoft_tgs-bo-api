using System;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Report;

public class GetAgentDividendDetailsReportViewRequest : PageableRequest
{
    public Guid AgentId { get; set; }

    public decimal AgentDividendPercentage { get; init; }

    public DateTimeOffset From { get; set; }

    public DateTimeOffset To { get; set; }
}