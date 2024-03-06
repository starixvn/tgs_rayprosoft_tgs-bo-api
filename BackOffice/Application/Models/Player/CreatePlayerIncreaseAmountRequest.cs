#nullable enable

using System;
using System.Collections.Generic;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.Player;

public record CreatePlayerIncreaseAmountRequest
{
    public string? PlayerCode { get; set; }

    public bool IncludeInNetProfit { get; set; }

    public PlayerIncreaseTransactionType TransactionType { get; set; }

    public decimal Amount { get; set; }

    public string? Remark { get; set; }

    public decimal BucketAmount { get; set; }

    public IEnumerable<string> BucketTypes { get; set; } = [];
}
