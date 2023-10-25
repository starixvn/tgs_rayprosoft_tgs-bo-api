using System;
using System.Collections.Generic;
using TGS.Core.Models.Enums;

#nullable enable

namespace TGS.Services.BackOffice.Domain;
public record PlayerBalanceAdjustmentApplyInfo(
    Guid BrandId,
    string PlayerCode,
    Guid WalletId,
    AdjustmentType AdjustmentType,
    PlayerAdjustmentTransactionType TransactionType,
    AdjustTransferType TransferType,
    decimal Amount,
    decimal BucketAmount,
    bool IncludeInNetProfit,
    string? Remark,
    IEnumerable<string> BucketTypes);
