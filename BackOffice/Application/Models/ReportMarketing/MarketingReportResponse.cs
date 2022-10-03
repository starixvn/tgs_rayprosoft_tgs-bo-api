using System.Collections.Generic;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportMarketing;

public class MarketingReportResponse : MessageResponse
{
    public IEnumerable<MarketingReportModel> Items { get; init; }

    public MarketingReportTotalModel Summary { get; init; }

    public int TotalCount { get; init; }
}
