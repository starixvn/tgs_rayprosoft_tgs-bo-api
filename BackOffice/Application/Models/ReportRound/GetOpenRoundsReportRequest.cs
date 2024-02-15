using System;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.ReportRound;

public class GetOpenRoundsReportRequest : PageableRequest
{
    public string ExternalRoundId { get; init; }

    public DateTimeOffset From { get; init; }

    public Guid? GameProviderId { get; init; }

    public DateTimeOffset To { get; init; }
}
