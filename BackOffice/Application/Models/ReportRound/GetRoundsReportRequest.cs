#nullable enable

using System;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.ReportRound;

public class GetRoundsReportRequest : PageableRequest
{
    public Guid BrandId { get; init; }

    public DateTimeOffset? ClosedFrom { get; init; }

    public DateTimeOffset? ClosedTo { get; init; }

    public string? ExternalRoundId { get; init; }

    public DateTimeOffset? From { get; init; }

    public Guid? GameProviderId { get; init; }

    public GameType? GameType { get; init; }

    public string? PlayerCode { get; init; }

    public Guid? RoundId { get; init; }

    public ClientRoundStatus? Status { get; init; }

    public DateTimeOffset? To { get; init; }

    public string? Username { get; init; }
}
