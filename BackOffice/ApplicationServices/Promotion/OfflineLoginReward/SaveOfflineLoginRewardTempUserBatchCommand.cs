using System;
using System.Collections.Generic;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion.OfflineLoginReward;

public record SaveOfflineLoginRewardTempUserBatchCommand(
    Guid BrandId,
    IEnumerable<string> UserNames);

