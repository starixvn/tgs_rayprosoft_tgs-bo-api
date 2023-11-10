using System;
using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.ViewModels;

public class CreateAgentReduceAmountViewModel
{
    public AccountType? AccountType { get; set; }

    public Guid AgentId { get; set; }

    public decimal Amount { get; set; }

    public string Remark { get; set; }

    public string TargetName { get; set; }

    public AgentReduceTransactionType TransactionType { get; set; }
}
