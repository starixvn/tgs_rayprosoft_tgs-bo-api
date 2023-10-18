using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

public record SetPlayerStatusCommand(
    string PlayerCode,
    PlayerStatus Status,
    string ModfiyUserId,
    string ModifyUsername);
