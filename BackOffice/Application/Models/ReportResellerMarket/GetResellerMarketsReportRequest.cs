using System;
using TGS.Services.BackOffice.Domain;

#nullable enable

namespace TGS.Services.BackOffice.Application.Models.ReportResellerMarket;

public class GetResellerMarketsReportRequest : PageableRequest
{
    public string? CurrencyCode { get; init; }

    public string[]? CurrencyCodes { get; init; }

    public DateTimeOffset From { get; init; }

    public Guid ResellerId { get; init; }

    public DateTimeOffset To { get; init; }
}
