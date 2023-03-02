using System;
using System.Collections.Generic;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;

public record SaveOfflineTempUserBatchCommand(
    Guid BrandId,
    IEnumerable<string> UserNames);

