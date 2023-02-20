using System;
using System.Collections.Generic;

namespace TGS.Services.BackOffice.Domain;

public record OfflineLoginRewardTempUserBatch(
    Guid BrandId,
    IEnumerable<string> UserNames);