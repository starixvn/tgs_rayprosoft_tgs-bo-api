using System;

namespace TGS.Services.BackOffice.Application.Models.ReportVip;

public record VipReportModel
{
    public Guid BrandId { get; init; }

    public string Username { get; init; }

    public string PlayerCode { get; init; }

    public int? Level { get; init; }

    public decimal DepositAmount { get; init; }

    public decimal WithdrawalAmount { get; init; }

    public decimal BonusAmount { get; init; }

    public decimal CompanyAdjustAmount { get; init; }
    public decimal BonusAdjustAmount { get; init; }

    public decimal RebateAmount { get; init; }

    public decimal BetAmount { get; init; }

    public decimal ValidBetAmount { get; init; }

    public decimal Turnover { get; init; }

    public decimal WinLossAmount { get; init; }

    public decimal NetProfit { get; init; }
}
