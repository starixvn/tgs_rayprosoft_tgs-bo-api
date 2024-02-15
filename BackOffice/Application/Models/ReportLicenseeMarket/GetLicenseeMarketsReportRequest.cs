#nullable enable

using System;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.ReportLicenseeMarket;

public class GetLicenseeMarketsReportRequest : PageableRequest
{
    public string? CurrencyCode { get; init; }

    public string[]? CurrencyCodes { get; init; }

    public DateTimeOffset From { get; init; }

    public Guid LicenseeId { get; init; }

    public DateTimeOffset To { get; init; }
}
