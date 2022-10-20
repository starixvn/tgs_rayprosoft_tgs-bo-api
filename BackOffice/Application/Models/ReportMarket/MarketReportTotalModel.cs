using System.Text.Json.Serialization;

namespace TGS.Services.BackOffice.Application.Models.ReportMarket;

public record MarketReportTotalModel(
    int RegisteredCount,
    int FirstFundInCount,
    int DepositCount,
    int ActivePlayer,
    int LoginPlayer,
    [property: JsonPropertyName("depositAmount")] decimal DepositAmountSummary,
    [property: JsonPropertyName("depositHandlingFee")] decimal DepositHandlingFeeSummary,
    [property: JsonPropertyName("withdrawalAmount")] decimal WithdrawalAmountSummary,
    [property: JsonPropertyName("companyAdjustAmount")] decimal CompanyAdjustAmountSummary,
    [property: JsonPropertyName("bonusAdjustAmount")] decimal BonusAdjustAmountSummary,
    [property: JsonPropertyName("bonusAmount")] decimal BonusAmountSummary,
    [property: JsonPropertyName("betAmount")] decimal BetAmountSummary,
    [property: JsonPropertyName("validBetAmount")] decimal ValidBetAmountSummary,
    [property: JsonPropertyName("turnover")] decimal TurnoverSummary,
    [property: JsonPropertyName("winLossAmount")] decimal WinLossAmountSummary,
    [property: JsonPropertyName("netProfit")] decimal NetProfitSummary,
    [property: JsonPropertyName("additionalExpenses")] decimal AdditionalExpensesSummary,
    [property: JsonPropertyName("platformCoveredBonus")] decimal PlatformCoveredBonusSummary,
    [property: JsonPropertyName("platformCoveredBonusAdjust")] decimal PlatformCoveredBonusAdjustSummary);
