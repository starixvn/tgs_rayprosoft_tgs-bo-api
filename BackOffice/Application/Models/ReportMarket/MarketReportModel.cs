using System;

namespace TGS.Services.BackOffice.Application.Models.ReportMarket;

public record MarketReportModel
{
    public DateTimeOffset ReportDate { get; init; }

    public int RegisteredCount { get; init; }

    public int FirstFundInCount { get; init; }

    public int DepositCount { get; init; }

    public int ActivePlayer { get; init; }

    public int LoginPlayer { get; init; }

    public decimal DepositAmount { get; init; }

    public decimal DepositHandlingFee { get; init; }

    public decimal WithdrawalAmount { get; init; }

    public decimal CompanyAdjustAmount { get; init; }

    public decimal BonusAdjustAmount { get; init; }

    public decimal BonusAmount { get; init; }

    public decimal BetAmount { get; init; }

    public decimal ValidBetAmount { get; init; }

    public decimal Turnover { get; init; }

    public decimal WinLossAmount { get; init; }

    public decimal NetProfit { get; init; }

    public decimal AdditionalExpenses { get; init; }

    public decimal PlatformCoveredBonus { get; init; }

    public decimal PlatformCoveredBonusAdjust { get; init; }
}
