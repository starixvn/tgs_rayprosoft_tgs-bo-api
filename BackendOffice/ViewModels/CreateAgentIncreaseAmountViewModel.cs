using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.ViewModels;

public class CreateAgentIncreaseAmountViewModel
{
    public decimal Amount { get; set; }

    public string Remark { get; set; }

    public AgentIncreaseTransactionType TransactionType { get; set; }
}
