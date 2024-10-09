#nullable enable

using System.Collections.Generic;

namespace TGS.Services.BackOffice.ApplicationServices.StationLetters;

public record SendAllPlayersLetterCommand(
    string BrandCode,
    IEnumerable<string> PlayerCodes,
    string Subject,
    string Content,
    string Operator);
