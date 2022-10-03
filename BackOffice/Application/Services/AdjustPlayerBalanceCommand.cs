using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGS.Core.Models.Enums;

#nullable enable

namespace TGS.Services.BackOffice.Application.Services;
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
