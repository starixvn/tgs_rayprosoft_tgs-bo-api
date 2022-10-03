using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.ViewModels;

public class BatchAdjustBalanceProcessConditionViewModel
{
    public bool IncludeInNetProfit { get; init; }

    public PlayerAdjustmentTransactionType TransactionType { get; init; }
}
