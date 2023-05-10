using System;

namespace TGS.Services.BackOffice.ApplicationServices.StationLetters;
public record ReplyLetterCommand(
    string BrandCode,
    string PlayerCode,
    Guid LetterId,
    string Content);
