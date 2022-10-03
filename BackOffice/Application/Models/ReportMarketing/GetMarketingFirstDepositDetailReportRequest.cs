using System;

namespace TGS.Services.BackOffice.Application.Models.ReportMarketing;

public record GetMarketingFirstDepositDetailReportRequest
{
    public Guid BrandId { get; init; }
    public string AgentUsername { get; init; } = default!;
    public DateTimeOffset Date { get; init; }
}
