using System.Text.Json.Serialization;

namespace TGS.Services.BackOffice.Application.Models.ReportPlayer;

public record PlayerContributionReportTotalModel(
    [property: JsonPropertyName("depositAmount")] decimal DepositAmountSummary,
    [property: JsonPropertyName("depositHandlingFee")] decimal DepositHandlingFeeSummary,
    [property: JsonPropertyName("withdrawalAmount")] decimal WithdrawalAmountSummary,
    [property: JsonPropertyName("companyAdjustAmount")] decimal CompanyAdjustAmountSummary,
    [property: JsonPropertyName("bonusAdjustAmount")] decimal BonusAdjustAmountSummary,
    [property: JsonPropertyName("agentAdjustAmount")] decimal AgentAdjustAmountSummary,
    [property: JsonPropertyName("bonusAmount")] decimal BonusAmountSummary,
    [property: JsonPropertyName("betAmount")] decimal BetAmountSummary,
    [property: JsonPropertyName("validBetAmount")] decimal ValidBetAmountSummary,
    [property: JsonPropertyName("turnover")] decimal TurnoverSummary,
    [property: JsonPropertyName("winLossAmount")] decimal WinLossAmountSummary,
    [property: JsonPropertyName("netProfit")] decimal NetProfitSummary);
