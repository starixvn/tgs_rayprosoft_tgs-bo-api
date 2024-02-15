using System;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.ReportGameProviderRound;

public class GetGameProviderRoundsReportRequest : PageableRequest
{
    public string CurrencyCode { get; init; }

    public DateTimeOffset From { get; init; }

    public Guid? GameProviderId { get; init; }

    public DateTimeOffset To { get; init; }
}
