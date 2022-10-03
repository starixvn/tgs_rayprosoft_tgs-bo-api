using System;

#nullable enable

namespace TGS.Services.BackOffice.Domain;
public record BatchAdjustBalanceItemResult(
    int Id,
    bool IsSuccess = true,
    Exception? Exception = null);
