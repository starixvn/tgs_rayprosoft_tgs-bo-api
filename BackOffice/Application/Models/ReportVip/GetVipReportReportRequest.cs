using System;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.ReportVip;

public class GetVipReportReportRequest : PageableRequest
{
    public Guid BrandId { get; init; }

    public DateTimeOffset From { get; init; }

    public int? Level { get; init; }

    public DateTimeOffset To { get; init; }

    public string Username { get; init; }
}
