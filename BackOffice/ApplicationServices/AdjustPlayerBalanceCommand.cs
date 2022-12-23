using System;
using TGS.Core.Models.Enums;

#nullable enable

namespace TGS.Services.BackOffice.ApplicationServices;
public record AdjustPlayerBalanceCommand(
    string PlayerCode,
    AdjustmentType AdjustmentType,
    PlayerAdjustmentTransactionType PlayerAdjustmentTransactionType,
    decimal Amount,
    decimal BucketAmount,
    string? Remark,
    bool IncludeInNetProfit,
    Guid ModifyUserId,
    string ModifyUser);
