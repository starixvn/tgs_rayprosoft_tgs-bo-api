using System;
using System.Collections.Generic;
using TGS.Core.Models.Enums;

#nullable enable

namespace TGS.Services.BackOffice.ApplicationServices.Adjust;
public record AdjustPlayerBalanceCommand(
    string PlayerCode,
    AdjustmentType AdjustmentType,
    PlayerAdjustmentTransactionType PlayerAdjustmentTransactionType,
    decimal Amount,
    decimal BucketAmount,
    string? Remark,
    bool IncludeInNetProfit,
    IEnumerable<string> BucketTypes,
    Guid ModifyUserId,
    string ModifyUser);
