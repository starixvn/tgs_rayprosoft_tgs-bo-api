using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportMarket;

public class GetMarketsReportResponse : MessageResponse
{
    public int TotalCount { get; init; }

    public MarketReportModel[] Items { get; init; }

    public MarketReportTotalModel Total { get; init; }
}