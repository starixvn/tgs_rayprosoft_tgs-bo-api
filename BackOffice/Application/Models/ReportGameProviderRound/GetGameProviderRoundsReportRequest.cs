using System;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportGameProviderRound;

public class GetGameProviderRoundsReportRequest : PageableRequest
{
    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }

    public Guid? GameProviderId { get; init; }

    public string CurrencyCode { get; init; }
}