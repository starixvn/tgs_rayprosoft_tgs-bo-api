#nullable enable

using System;

namespace TGS.Services.BackOffice.Domain;
public record BatchAdjustBalanceItem(
    int Id,
    Guid BrandId,
    Guid WalletId,
    string PlayerCode,
    string UserName,
    decimal AdjustAmount,
    decimal Turnover,
    string? Remark);
