#nullable enable

using System;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.ReportRound;

public class GetBetRecordsReportRequest : PageableRequest
{
    public Guid BrandId { get; init; }

    public DateTimeOffset From { get; init; }

    public Guid? GameProviderId { get; init; }

    public GameType? GameType { get; init; }

    public string PlayerCode { get; init; } = default!;

    public ClientRoundStatus? Status { get; init; }

    public DateTimeOffset To { get; init; }
}
