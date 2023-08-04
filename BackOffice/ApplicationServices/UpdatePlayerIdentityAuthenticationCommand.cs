namespace TGS.Services.BackOffice.ApplicationServices;

public record UpdatePlayerIdentityAuthenticationCommand(string PlayerCode, bool IdentityAuthentication);
