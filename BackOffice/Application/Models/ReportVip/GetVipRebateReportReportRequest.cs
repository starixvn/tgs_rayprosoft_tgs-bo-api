using System;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportVip;

public class GetVipRebateReportReportRequest : PageableRequest
{
    public Guid BrandId { get; init; }

    public string Username { get; set; }

    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }
}