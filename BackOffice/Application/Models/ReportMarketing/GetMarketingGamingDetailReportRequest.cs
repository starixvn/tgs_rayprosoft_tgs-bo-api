#nullable enable

using System;
using Tgs.Report.Client.Models;

namespace TGS.Services.BackOffice.Application.Models.ReportMarketing;

public record GetMarketingGamingDetailReportRequest : PageableRequest
{
    public Guid BrandId { get; init; }
    public string AgentUsername { get; init; } = default!;
    public string? PlayerUsername { get; init; }
    public DateOnly ReportDate { get; init; }
}
