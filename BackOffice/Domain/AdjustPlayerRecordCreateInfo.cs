using System;
using TGS.Core.Models.Enums;

#nullable enable

namespace TGS.Services.BackOffice.Domain;
public record AdjustPlayerRecordCreateInfo(
    Guid BrandId,
    string PlayerCode,
    AdjustmentType AdjustmentType,
    PlayerAdjustmentTransactionType TransactionType,
    AdjustTransferType TransferType,
    decimal Amount,
    decimal BucketAmount,
    bool IncludeInNetProfit,
    string? Remark,
    Guid? TransactionId = null,
    string? TargetAgentUserName = null);
