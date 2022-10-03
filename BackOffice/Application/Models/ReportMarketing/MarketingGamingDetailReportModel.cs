namespace TGS.Services.BackOffice.Application.Models.ReportMarketing;

public record MarketingGamingDetailReportModel
{
    public string Username { get; init; } = default!;
    public string GameProviderName { get; init; } = default!;
    public string SubGameProviderName { get; init; } = default!;
    public decimal WinLossAmount { get; init; }
}