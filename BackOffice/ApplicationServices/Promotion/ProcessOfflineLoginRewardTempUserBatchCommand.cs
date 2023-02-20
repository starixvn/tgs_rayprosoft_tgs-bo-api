using System;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

public record ProcessOfflineLoginRewardTempUserBatchCommand(
    Guid BatchId);