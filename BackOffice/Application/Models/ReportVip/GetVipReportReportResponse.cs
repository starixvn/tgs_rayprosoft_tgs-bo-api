using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportVip;

public class GetVipReportReportResponse : MessageResponse
{
    public VipReportModel[] Items { get; init; }

    public VipReportTotalModel Total { get; init; }

    public int TotalCount { get; init; }
}
