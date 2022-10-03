using System;

namespace TGS.Services.BackOffice.Application.Models.ReportMarketing;

public record MarketingReportModel
{
    public Guid AgentId { get; init; }

    public string AgentUsername { get; set; }

    public DateOnly ReportDate { get; init; }

    public int RegisterCount { get; init; }

    public int FirstDepositCount { get; init; }

    public int GamingCount { get; init; }
}
