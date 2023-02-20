using System;
using System.Collections.Generic;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

public record SaveOfflineLoginRewardTempUserBatchCommand(
    Guid BrandId, 
    IEnumerable<string> UserNames);

