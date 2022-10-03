using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportVip;

public class GetVipReportReportResponse : MessageResponse
{
    public int TotalCount { get; init; }

    public VipReportModel[] Items { get; init; }

    public VipReportTotalModel Total { get; init; }
}