#nullable enable

using System.Collections.Generic;

namespace TGS.Services.BackOffice.ApplicationServices.StationLetters;
public record SendLetterCommand(
    string BrandCode,
    IEnumerable<string> PlayerCodes,
    string Subject,
    string Content,
    string Operator);
