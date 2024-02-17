using System;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.ReportAgent;

public class GetAgentContributionsReportRequest : PageableRequest
{
    public Guid? AgentId { get; init; }

    public Guid BrandId { get; init; }

    public DateTimeOffset From { get; init; }

    public bool IncludeAgents { get; init; }

    public DateTimeOffset To { get; init; }

    public AgentType? Type { get; init; }

    public string Username { get; init; }
}
