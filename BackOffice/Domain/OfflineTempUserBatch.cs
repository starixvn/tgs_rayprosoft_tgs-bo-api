#nullable enable

using System;
using System.Collections.Generic;

namespace TGS.Services.BackOffice.Domain;

public record OfflineTempUserBatch(
    Guid BrandId,
    string? ClientToken,
    IEnumerable<string> UserNames,
    Guid ModifyUserId);