using System;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportVip;

public class GetVipReportReportRequest : PageableRequest
{
    public Guid BrandId { get; init; }

    public string Username { get; init; }

    public int? Level { get; init; }

    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }
}