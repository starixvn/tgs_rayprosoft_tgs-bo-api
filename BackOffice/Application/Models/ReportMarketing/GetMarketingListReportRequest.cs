#nullable enable

using System;

namespace TGS.Services.BackOffice.Application.Models.ReportMarketing;

public record GetMarketingListReportRequest
{
    public Guid BrandId { get; init; }
    public string? AgentUsername { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}
