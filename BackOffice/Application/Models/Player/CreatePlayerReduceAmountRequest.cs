#nullable enable

using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.Player;

public record CreatePlayerReduceAmountRequest
{
    public string? PlayerCode { get; set; }

    public bool IncludeInNetProfit { get; set; }

    public PlayerReduceTransactionType TransactionType { get; set; }

    public decimal Amount { get; set; }

    public string? Remark { get; set; }

    public decimal BucketAmount { get; set; }
}
