namespace TGS.Services.BackOffice.Application.Models.ReportMarket;

public record MarketReportTotalModel(
    int RegisteredCount,
    int FirstFundInCount,
    int DepositCount,
    int ActivePlayer,
    int LoginPlayer,
    decimal DepositAmount,
    decimal WithdrawalAmount,
    decimal CompanyAdjustAmount,
    decimal BonusAdjustAmount,
    decimal BonusAmount,
    decimal BetAmount,
    decimal ValidBetAmount,
    decimal Turnover,
    decimal WinLossAmount,
    decimal NetProfit,
    decimal AdditionalExpenses,
    decimal PlatformCoveredBonus,
    decimal PlatformCoveredBonusAdjust
);