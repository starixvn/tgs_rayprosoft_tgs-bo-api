using System;

#nullable enable

namespace TGS.Services.BackOffice.Domain;
public record PlayerBalanceTransferApplyInfo(
    Guid BrandId,
    string PlayerCode,
    Guid WalletId,
    decimal Amount,
    decimal BucketAmount,
    bool IncludeInNetProfit,
    string? Remark,
    Guid TargetAgent,
    Guid TargetAgentWalletId);
