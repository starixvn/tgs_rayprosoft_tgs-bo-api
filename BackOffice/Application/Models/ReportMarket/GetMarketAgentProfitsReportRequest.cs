using System;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.ReportMarket;

public class GetMarketAgentProfitsReportRequest : PageableRequest
{
    public Guid BrandId { get; init; }
    public DateTimeOffset Date { get; init; }
}