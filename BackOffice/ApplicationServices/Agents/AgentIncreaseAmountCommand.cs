using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;
public record AgentIncreaseAmountCommand(
    Guid AgentId,
    decimal Amount,
    string Remark,
    AgentAdjustmentTransactionType TransactionType,
    Guid UserId,
    string UserName);
