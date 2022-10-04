using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.ApplicationServices.BatchAdjust;
public record ProcessBatchCommand(
    Guid BrandId,
    Guid BatchId,
    PlayerAdjustmentTransactionType AdjustTransferType,
    bool IncludeInNetProfit,
    Guid ModifyUserId,
    string ModifyUserName);
