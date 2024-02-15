using System;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.ReportPlayer;

public class GetPlayerContributionsReportRequest : PageableRequest
{
    public Guid BrandId { get; init; }

    public DateTimeOffset From { get; init; }

    public bool? HasUpperAgent { get; init; }

    public int? Level { get; init; }

    public DateTimeOffset To { get; init; }

    public string Username { get; init; } = default!;
}
