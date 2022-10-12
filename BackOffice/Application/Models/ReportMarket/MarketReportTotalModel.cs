using System.Text.Json.Serialization;

namespace TGS.Services.BackOffice.Application.Models.ReportMarket;

public record MarketReportTotalModel(
    int RegisteredCount,
    int FirstFundInCount,
    int DepositCount,
    int ActivePlayer,
    int LoginPlayer,
    [property: JsonPropertyName("depositAmount")] decimal DepositSummary,
    [property: JsonPropertyName("withdrawalAmount")] decimal WithdrawalSummary,
    [property: JsonPropertyName("companyAdjustAmount")] decimal CompanyAdjustSummary,
    [property: JsonPropertyName("bonusAdjustAmount")] decimal BonusAdjustSummary,
    [property: JsonPropertyName("bonusAmount")] decimal BonusSummary,
    [property: JsonPropertyName("betAmount")] decimal BetSummary,
    [property: JsonPropertyName("validBetAmount")] decimal ValidBetSummary,
    [property: JsonPropertyName("turnover")] decimal TurnoverSummary,
    [property: JsonPropertyName("winLossAmount")] decimal WinLossSummary,
    [property: JsonPropertyName("netProfit")] decimal NetProfitSummary,
    [property: JsonPropertyName("additionalExpenses")] decimal AdditionalExpensesSummary,
    [property: JsonPropertyName("platformCoveredBonus")] decimal PlatformCoveredBonusSummary,
    [property: JsonPropertyName("platformCoveredBonusAdjust")] decimal PlatformCoveredBonusAdjustSummary,
    [property: JsonPropertyName("depositHandlingFee")] decimal DepositHandlingFeeSummary);
