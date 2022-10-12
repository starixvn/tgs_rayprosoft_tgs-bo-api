namespace TGS.Services.BackOffice.Application.Models.ReportResellerMarket;

public record ResellerMarketTotalReportModel
{
    public string CurrencyCode { get; init; } = default!;

    public int RegisteredCount { get; init; }

    public int FirstFundInCount { get; init; }

    public decimal DepositAmount { get; init; }

    public decimal DepositHandlingFee { get; init; }

    public decimal WithdrawalAmount { get; init; }

    public decimal CompanyAdjustAmount { get; init; }

    public decimal BonusAmount { get; init; }

    public decimal BetAmount { get; init; }

    public decimal Turnover { get; init; }

    public decimal WinLossAmount { get; init; }

    public decimal NetProfit { get; init; }
}
