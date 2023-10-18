namespace TGS.Services.BackOffice.ApplicationServices.Players;

public record SetPlayerParentPlayerCommand(
    string PlayerCode,
    string ParentUsername,
    string ModifyUsername);
