using System;

namespace TGS.Services.BackOffice.Application.Models.ReportAgent;

public record AgentDividendDetailReportModel
{
    public Guid Id { get; init; }

    public string Username { get; init; } = default!;

    public DateTimeOffset SettlementDate { get; init; }

    public DateTimeOffset SettlementDateFrom { get; init; }

    public DateTimeOffset SettlementDateTo { get; init; }

    public decimal AgentDividendAmount { get; init; }
}