namespace TGS.Services.BackOffice.Application.Models.ReportVip;

public record VipReportTotalModel(
    decimal DepositAmount,
    decimal WithdrawalAmount,
    decimal CompanyAdjustAmount,
    decimal BonusAdjustAmount,
    decimal RebateAmount,
    decimal BonusAmount,
    decimal BetAmount,
    decimal ValidBetAmount,
    decimal Turnover,
    decimal WinLossAmount,
    decimal NetProfit
);