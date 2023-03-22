using System;

namespace TGS.Services.BackOffice.Application.Models.ReportResellerMarket;

public record ResellerMarketDataReportModel
{
    public Guid BrandId { get; init; }

    public string BrandCode { get; init; } = default!;

    public string CurrencyCode { get; init; } = default!;

    public decimal DepositAmount { get; init; }

    public decimal DepositHandlingFee { get; init; }

    public decimal WithdrawalAmount { get; init; }

    public decimal CompanyAdjustAmount { get; init; }

    public decimal BonusAmount { get; init; }

    public decimal BetAmount { get; init; }

    public decimal Turnover { get; init; }

    public decimal WinLossAmount { get; init; }

    public decimal NetProfit { get; init; }

    public decimal AgentProfit { get; init; }
}
