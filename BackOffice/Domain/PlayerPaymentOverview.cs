using System;

namespace TGS.Services.BackOffice.Domain;
public record PlayerPaymentOverview(
    string PlayerCode,
    decimal? FirstDepositAmount,
    decimal? FirstDepositHandlingFee,
    DateTime? FirstDepositOn,
    decimal? FirstWithdrawAmount,
    decimal? FirstWithdrawHandlingFee,
    DateTime? FirstWithdrawOn,
    DateOnly RecordedBy,
    decimal TotalDepositAmount,
    decimal TotalWithdrawAmount,
    int TotalDepositCount,
    int TotalWithdrawCount);
