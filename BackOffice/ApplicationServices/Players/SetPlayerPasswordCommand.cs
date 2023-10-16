namespace TGS.Services.BackOffice.ApplicationServices.Players;

public record SetPlayerPasswordCommand(
    string PlayerCode,
    string Password,
    string ModifyUserId,
    string ModifyUsername);
