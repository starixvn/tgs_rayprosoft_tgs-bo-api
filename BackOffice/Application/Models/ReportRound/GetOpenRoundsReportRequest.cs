using System;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportRound;

public class GetOpenRoundsReportRequest : PageableRequest
{
    public string ExternalRoundId { get; init; }

    public Guid? GameProviderId { get; init; }

    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }
}