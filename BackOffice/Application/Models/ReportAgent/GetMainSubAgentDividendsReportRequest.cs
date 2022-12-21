using System;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportAgent;

public class GetMainSubAgentDividendsReportRequest : PageableRequest
{
    public Guid BrandId { get; init; }

    public Guid? AgentId { get; init; }

    public string Username { get; init; } = default!;

    public AgentType? Type { get; init; }

    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }
}