using System;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Report;

public class GetMainSubAgentDividendsReportViewRequest : PageableRequest
{
    public Guid BrandId { get; init; }

    public Guid? AgentId { get; init; }

    public string Username { get; init; } = default!;

    public AgentType? Type { get; init; }

    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }
}