using System;
using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IAdjustAgentAccountService
{
    ValueTask<AdjustAgentRecord> CreateAsync(Guid transactionId, Guid brandId, Guid agentId,
        AdjustmentType adjustmentType,
        AgentAdjustmentTransactionType transactionType, AdjustTransferType transferType, decimal amount,
        string remark, string username, string targetAgentUsername = null);

    Task UpdateAdjustAgentRecordAsync(AdjustAgentRecord record);
}

public class AdjustAgentAccountService : BaseRepositoryOperations<ITgsRepository>, IAdjustAgentAccountService
{
    public AdjustAgentAccountService(ITgsRepository tgsRepository) : base(
        tgsRepository)
    {
    }

    public async ValueTask<AdjustAgentRecord> CreateAsync(Guid transactionId, Guid brandId, Guid agentId,
        AdjustmentType adjustmentType,
        AgentAdjustmentTransactionType transactionType, AdjustTransferType transferType, decimal amount,
        string remark, string username, string targetAgentUsername)
    {
        var vipLevel = new AdjustAgentRecord
        {
            TransactionId = transactionId,
            BrandId = brandId,
            AgentId = agentId,
            AdjustmentType = adjustmentType,
            Type = transactionType,
            TransferType = transferType,
            Amount = amount,
            Remark = remark,
            CreatedByUsername = username,
            TransferTarget = targetAgentUsername,
            CreatedOn = DateTimeOffset.Now
        };

        await CreateEntityAsync(vipLevel);

        return vipLevel;
    }

    public async Task UpdateAdjustAgentRecordAsync(AdjustAgentRecord record)
        => await UpdateEntityAsync(record);
}
