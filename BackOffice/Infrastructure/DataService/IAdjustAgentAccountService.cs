using System;
using System.Threading.Tasks;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IAdjustAgentAccountService
{
    ValueTask<AdjustAgentRecord> CreateAsync(
        Guid transactionId,
        Guid brandId,
        Guid agentId,
        AdjustmentType adjustmentType,
        AgentAdjustmentTransactionType transactionType,
        AdjustTransferType transferType,
        decimal amount,
        string remark,
        string username,
        string targetAgentUsername = null);

    Task UpdateAdjustAgentRecordAsync(AdjustAgentRecord record);
}
