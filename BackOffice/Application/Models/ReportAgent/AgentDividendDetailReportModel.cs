using System;

namespace TGS.Services.BackOffice.Application.Models.ReportAgent;

public record AgentDividendDetailReportModel
{
    public Guid Id { get; init; }

    public string Username { get; init; } = default!;

    public DateTimeOffset SettlementDate { get; init; }

    public DateTimeOffset SettlementDateFrom { get; init; }

    public DateTimeOffset SettlementDateTo { get; init; }

    public decimal LiveDealerWinLoss { get; init; }

    public decimal SlotWinLoss { get; init; }

    public decimal LotteryWinLoss { get; init; }

    public decimal CardGameWinLoss { get; init; }

    public decimal SportsWinLoss { get; init; }

    public decimal VirtualSportsWinLoss { get; init; }

    public decimal WinLossAmount { get; init; }

    public decimal AgentDividendAmount { get; init; }

    public decimal AgentDividendPercentage { get; init; }

    public decimal ExpenditureAmount { get; init; }
}