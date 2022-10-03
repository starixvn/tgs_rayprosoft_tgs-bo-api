using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportLicenseeMarket;

public record LicenseeMarketsReportResponse
{
    public PagedResponse<LicenseeMarketDataReportModel> Datas { get; init; }

    public LicenseeMarketTotalReportModel[] Totals { get; init; }
}