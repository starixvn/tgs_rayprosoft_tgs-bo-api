using System;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;

public record ProcessOfflineLoginRewardTempUserBatchCommand(
    Guid BatchId);