using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

public record AgentReduceAmountCommand(
    Guid AgentId,
    decimal Amount,
    string Remark,
    string TargetName,
    AgentAdjustmentTransactionType TransactionType,
    AccountType? AccountType,
    string Username,
    Guid UserId);
