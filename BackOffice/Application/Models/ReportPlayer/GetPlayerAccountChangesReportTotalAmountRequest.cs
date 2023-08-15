using System;
using TGS.Services.BackOffice.Domain.Enums;

namespace TGS.Services.BackOffice.Application.Models.ReportPlayer;

public class GetPlayerAccountChangesReportTotalAmountRequest
{
    public TransactionType? AccountChangeType { get; init; }

    public Guid BrandId { get; init; }

    public DateTimeOffset From { get; init; }

    public string PlayerCode { get; init; } = default!;

    public DateTimeOffset To { get; init; }
}
