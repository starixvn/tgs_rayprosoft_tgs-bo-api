using System;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportBrandGameProviderRound;

public class GetBrandGameProviderRoundsReportRequest : PageableRequest
{
    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }

    public Guid BrandId { get; init; }

    public string GameProviderCode { get; init; } = default!;

    public GameType? GameType { get; init; }
}