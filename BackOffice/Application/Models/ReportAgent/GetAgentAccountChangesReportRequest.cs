using System;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportAgent;

public class GetAgentAccountChangesReportRequest : PageableRequest
{
    public Guid BrandId { get; set; }

    public Guid AgentId { get; set; }

    public TransactionType? TransactionType { get; set; }

    public DateTimeOffset From { get; set; }

    public DateTimeOffset To { get; set; }
}