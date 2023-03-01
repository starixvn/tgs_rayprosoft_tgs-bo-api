using System;
using System.Collections.Generic;

namespace TGS.Services.BackOffice.Domain;

public record OfflineTempUserBatch(
    Guid BrandId,
    IEnumerable<string> UserNames);