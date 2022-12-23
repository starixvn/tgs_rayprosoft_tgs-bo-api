using System;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportMarket;

public class GetMarketsReportRequest : PageableRequest
{
    public Guid BrandId { get; init; }

    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }
}
