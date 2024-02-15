using System;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Report;

public class GetMainSubAgentDividendsReportViewRequest : PageableRequest
{
    public Guid? AgentId { get; init; }

    public Guid BrandId { get; init; }

    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }

    public AgentType? Type { get; init; }

    public string Username { get; init; } = default!;
}
