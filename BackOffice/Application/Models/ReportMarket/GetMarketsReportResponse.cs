using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportMarket;

public class GetMarketsReportResponse : MessageResponse
{
    public MarketReportModel[] Items { get; init; }

    public MarketReportTotalModel Total { get; init; }

    public int TotalCount { get; init; }
}
