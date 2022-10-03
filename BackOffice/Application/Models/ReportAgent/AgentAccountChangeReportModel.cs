using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.ReportAgent;

public record AgentAccountChangeReportModel
{
    public Guid TransactionId { get; init; }

    public decimal Amount { get; init; }

    public decimal Balance { get; init; }

    public TransactionType TransactionType { get; init; }

    public DateTimeOffset RecordedOn { get; init; }
}