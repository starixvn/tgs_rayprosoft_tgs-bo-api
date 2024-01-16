using System;
using Tgs.VipLevelService.Common;

namespace TGS.Apis.BackendOffice.ViewModels;

public record PlayerVipAdjustViewModel(
    PlayerVipLevelLogType Type, 
    int OldLevel,
    PlayerVipLevelAutoState OldAutoState,
    int NewLevel,
    PlayerVipLevelAutoState NewAutoState,
    DateTimeOffset CreatedOn,
    string CreatedUsername,
    string Remark);
