using System;

#nullable enable

namespace TGS.Services.BackOffice.ApplicationServices;
public record PlayerTransferBalanceToParentAgentCommand(
    string PlayerCode,
    decimal Amount,
    decimal BucketAmount,
    string? Remark,
    bool IncludeInNetProfit,
    Guid ModifyUserId);
