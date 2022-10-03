using System;

namespace TGS.Services.BackOffice.Domain;
public record PlayerDailyPaymentItem(
    string PlayerCode,
    string PlayerName,
    Guid AgentId,
    bool? IsFirstDeposit,
    bool? UnSecondDeposit,
    DateTime FirstDepositOn,
    decimal FirstDepositAmount,
    decimal TotalDepositAmount,
    decimal TotalWithdrawAmount,
    string RegisterIp);
    