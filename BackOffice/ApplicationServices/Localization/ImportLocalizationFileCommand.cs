using System.IO;

namespace TGS.Services.BackOffice.ApplicationServices.Localization;

public record ImportLocalizationFileCommand(string SystemCode, Stream FileStream);
