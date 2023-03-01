using System;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion.OfflineLoginReward;

public record ProcessOfflineLoginRewardTempUserBatchCommand(
    Guid BatchId);