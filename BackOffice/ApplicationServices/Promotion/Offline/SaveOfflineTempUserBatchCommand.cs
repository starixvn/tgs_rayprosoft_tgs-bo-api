#nullable enable

using System;
using System.Collections.Generic;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;

public record SaveOfflineTempUserBatchCommand(
    Guid BrandId,
    string? ClientToken,
    IEnumerable<string> UserNames,
    Guid ModifyUserId);

