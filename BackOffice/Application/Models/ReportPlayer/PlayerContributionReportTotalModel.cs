namespace TGS.Services.BackOffice.Application.Models.ReportPlayer;

public record PlayerContributionReportTotalModel(
    decimal DepositAmount,
    decimal WithdrawalAmount,
    decimal CompanyAdjustAmount,
    decimal BonusAdjustAmount,
    decimal AgentAdjustAmount,
    decimal BonusAmount,
    decimal BetAmount,
    decimal ValidBetAmount,
    decimal Turnover,
    decimal WinLossAmount,
    decimal NetProfit,
    decimal DepositHandlingFee);
