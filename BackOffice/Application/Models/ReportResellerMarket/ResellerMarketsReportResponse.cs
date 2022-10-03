using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportResellerMarket;

public record ResellerMarketsReportResponse
{
    public PagedResponse<ResellerMarketDataReportModel> Datas { get; init; }

    public ResellerMarketTotalReportModel[] Totals { get; init; }
}