namespace TGS.Services.BackOffice.Application.Models.ReportBrandGameProviderRound;

public record BrandGameProviderTotalReportModel
{
    public string CurrencyCode { get; init; } = default!;

    public int TransactionCount { get; init; }

    public decimal BetAmount { get; init; }

    public decimal ValidBetAmount { get; init; }

    public decimal Turnover { get; init; }

    public decimal CompanyWinLoss { get; init; }

    public int PlayerCount { get; init; }
}
