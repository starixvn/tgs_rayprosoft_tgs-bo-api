using System;

#nullable enable

namespace TGS.BackOffice.Repositories;
internal record RealTimePlayerPaymentItem(
    string PlayerCode,
    Guid BrandId,
    string UserName,
    Guid AgentId,
    DateTime FirstDepositOn,
    decimal FirstDepositAmount,
    decimal TotalDepositAmount,
    decimal TotalWithdrawAmount,
    bool IsUndepositAgain,
    bool? IsCurrentDeposit,
    string? RegisterIp);
