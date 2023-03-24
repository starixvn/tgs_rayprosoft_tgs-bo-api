using System.Text.Json.Serialization;

namespace TGS.Services.BackOffice.Application.Models.ReportResellerMarket;

public record ResellerMarketTotalReportModel
{
    public string CurrencyCode { get; init; } = default!;

    [JsonPropertyName("depositAmount")]
    public decimal DepositAmountSummary { get; init; }

    [JsonPropertyName("depositHandlingFee")]
    public decimal DepositHandlingFeeSummary { get; init; }

    [JsonPropertyName("withdrawalAmount")]
    public decimal WithdrawalAmountSummary { get; init; }

    [JsonPropertyName("companyAdjustAmount")]
    public decimal CompanyAdjustAmountSummary { get; init; }

    [JsonPropertyName("bonusAmount")]
    public decimal BonusAmountSummary { get; init; }

    [JsonPropertyName("betAmount")]
    public decimal BetAmountSummary { get; init; }

    [JsonPropertyName("turnover")]
    public decimal TurnoverSummary { get; init; }

    [JsonPropertyName("winLossAmount")]
    public decimal WinLossAmountSummary { get; init; }

    [JsonPropertyName("netProfit")]
    public decimal NetProfitSummary { get; init; }

    [property: JsonPropertyName("agentProfit")]
    public decimal AgentProfitSummary { get; init; }
}
