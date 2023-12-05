#nullable enable

using System;
using TGS.Services.BackOffice.Domain.Enums;

namespace TGS.Apis.BackendOffice.ViewModels;

public class PlayerAccountChangeViewModel
{
    public Guid TransactionId { get; set; }

    public decimal Amount { get; set; }
    public decimal LockAmount { get; set; }

    public decimal Balance { get; set; }
    public decimal LockedBalance { get; set; }

    public TransactionType TransactionType { get; set; }

    public DateTime RecordedOn { get; set; }

    public bool IncludeInNetProfit { get; set; }

    public string? Remark { get; set; }

    public string? ExternalTransactionId { get; set; }
}
