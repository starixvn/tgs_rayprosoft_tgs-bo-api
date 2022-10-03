using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.ReportPlayer;

public class GetPlayerAccountChangesReportTotalAmountRequest
{
    public AccountChangeType? AccountChangeType { get; init; }

    public Guid BrandId { get; init; }

    public DateTimeOffset From { get; init; }

    public string PlayerCode { get; init; } = default!;

    public DateTimeOffset To { get; init; }
}
