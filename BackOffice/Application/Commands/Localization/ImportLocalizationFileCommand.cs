using System.IO;

namespace TGS.Services.BackOffice.Application.Commands.Localization;

public record ImportLocalizationFileCommand(string SystemCode, Stream FileStream);
