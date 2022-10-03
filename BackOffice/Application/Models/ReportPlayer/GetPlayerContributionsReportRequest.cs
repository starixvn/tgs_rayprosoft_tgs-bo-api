using System;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportPlayer;

public class GetPlayerContributionsReportRequest : PageableRequest
{
    public Guid BrandId { get; init; }

    public string Username { get; init; } = default!;

    public int? Level { get; init; }

    public bool? HasUpperAgent { get; init; }

    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }
}