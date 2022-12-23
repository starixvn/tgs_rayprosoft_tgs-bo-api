using System;
using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IAdjustPlayerAccountService
{
    ValueTask<AdjustPlayerRecord> CreateAsync(Guid brandId, string playerCode, AdjustmentType adjustmentType,
        PlayerAdjustmentTransactionType transactionType, AdjustTransferType transferType, decimal amount,
        string remark, decimal bucketAmount, string username, bool includeInNetProfit,
        string targetAgentUsername = null);

    Task UpdateAdjustPlayerRecordAsync(AdjustPlayerRecord record);
}

public class AdjustPlayerAccountService : BaseRepositoryOperations<ITgsRepository>, IAdjustPlayerAccountService
{
    public AdjustPlayerAccountService(ITgsRepository tgsRepository) : base(
        tgsRepository)
    {
    }

    public async ValueTask<AdjustPlayerRecord> CreateAsync(Guid brandId, string playerCode,
        AdjustmentType adjustmentType,
        PlayerAdjustmentTransactionType transactionType, AdjustTransferType transferType, decimal amount,
        string remark, decimal bucketAmount, string username, bool includeInNetProfit, string targetAgentUsername)
    {
        var vipLevel = new AdjustPlayerRecord
        {
            BrandId = brandId,
            PlayerCode = playerCode,
            IncludeInNetProfit = includeInNetProfit,
            AdjustmentType = adjustmentType,
            Type = transactionType,
            TransferType = transferType,
            Amount = amount,
            Remark = remark,
            BucketAmount = bucketAmount,
            CreatedByUsername = username,
            TransferTarget = targetAgentUsername,
            CreatedOn = DateTimeOffset.Now
        };

        await CreateEntityAsync(vipLevel);

        return vipLevel;
    }

    public async Task UpdateAdjustPlayerRecordAsync(AdjustPlayerRecord record)
        => await UpdateEntityAsync(record);
}
