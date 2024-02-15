using System;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.ReportBrandGameProviderRound;

public class GetBrandGameProviderRoundsReportRequest : PageableRequest
{
    public Guid BrandId { get; init; }

    public DateTimeOffset From { get; init; }

    public string GameProviderCode { get; init; } = default!;

    public GameType? GameType { get; init; }

    public DateTimeOffset To { get; init; }
}
