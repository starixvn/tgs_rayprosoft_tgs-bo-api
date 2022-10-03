using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.ReportPlayer;

public record PlayerAccountChangeReportModel
{
    public Guid? TransactionId { get; init; }
    public Guid? GameActionId { get; init; }
    public decimal Amount { get; init; }
    public decimal Balance { get; init; }
    public TransactionType? TransactionType { get; init; }
    public TransactionGameActionType? GameActionType { get; init; }
    public DateTimeOffset RecordedOn { get; init; }
    public bool IncludeInNetProfit { get; init; }
    public string Remark { get; set; }
    public string ExternalTransactionId { get; init; }
}
