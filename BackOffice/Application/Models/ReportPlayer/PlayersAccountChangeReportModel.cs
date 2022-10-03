using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.ReportPlayer;

public record PlayersAccountChangeReportModel
{
    public string Username { get; init; } = default!;

    public decimal Amount { get; init; }

    public decimal BeforeBalance { get; init; }

    public decimal AfterBalance { get; init; }

    public TransactionType TransactionType { get; init; }

    public DateTimeOffset RecordedOn { get; init; }

    public string Operator { get; init; } = default!;

    public string Description { get; set; } = default!;
}