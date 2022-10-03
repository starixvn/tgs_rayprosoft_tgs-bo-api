using System;
using Tgs.Report.Client.Models;

#nullable enable

namespace TGS.Services.BackOffice.Application.Models.ReportPlayerDeposit;

public record GetPlayerDepositsReportRequest : PageableRequest
{
    public Guid BrandId { get; init; }
    public string? AgentUsername { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}
