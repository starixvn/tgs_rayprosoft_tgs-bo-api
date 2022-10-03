using System;

namespace TGS.Services.BackOffice.Application.Models.ReportMarketing;

public record MarketingRegisterDetailReportModel
{
    public string Username { get; init; } = default!;
    public DateTimeOffset? RegisteredOn { get; init; }
    public DateTimeOffset? LastLoginOn { get; init; }
    public int LoginCount { get; init; }
}
