using System;
using Tgs.VipLevelService.Common;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

public record SetPlayerVipLevelCommand(
    string PlayerCode,
    Guid ModifyUserId,
    string ModifyUsername,
    Guid VipId,
    PlayerVipLevelAutoState AutoState = PlayerVipLevelAutoState.Enable,
    string Remark = "");
