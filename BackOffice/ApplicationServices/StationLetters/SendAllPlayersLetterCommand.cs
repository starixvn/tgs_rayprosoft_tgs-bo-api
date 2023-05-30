namespace TGS.Services.BackOffice.ApplicationServices.StationLetters;

public record SendAllPlayersLetterCommand(
    string BrandCode,
    string Subject,
    string Content);
