#nullable enable

using System;

namespace TGS.Services.BackOffice.ApplicationServices.Adjust;
public record AdjustPlayerLockedAmountCommand(
    string PlayerCode,
    decimal LockAmount,
    string? Remark,
    Guid ModifyUserId);
