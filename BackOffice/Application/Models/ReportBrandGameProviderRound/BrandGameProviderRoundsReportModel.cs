using System;

namespace TGS.Services.BackOffice.Application.Models.ReportBrandGameProviderRound;

public record BrandGameProviderRoundsReportModel
{
    public Guid GameProviderId { get; init; }

    public string GameProviderName { get; init; } = default!;

    public string CurrencyCode { get; init; } = default!;

    public int TransactionCount { get; init; }

    public decimal BetAmount { get; init; }

    public decimal ValidBetAmount { get; init; }

    public decimal OriginalTurnover { get; init; }

    public decimal CompanyWinLoss { get; init; }
}
