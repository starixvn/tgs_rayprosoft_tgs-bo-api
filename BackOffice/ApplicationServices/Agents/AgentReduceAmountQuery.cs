using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

public record AgentReduceAmountQuery(
    Guid AgentId,
    decimal Amount,
    string Remark,
    string TargetName,
    AgentReduceTransactionType TransactionType,
    AccountType? AccountType,
    string Username,
    string UserId);
