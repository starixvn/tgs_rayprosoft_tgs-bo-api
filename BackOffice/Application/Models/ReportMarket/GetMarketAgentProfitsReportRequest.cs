using System;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportMarket;

public class GetMarketAgentProfitsReportRequest : PageableRequest
{
    public Guid BrandId { get; init; }
    public DateTimeOffset Date { get; init; }
}