using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using ClosedXML.Graphics;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Localization;

internal class ImportLocalizationFileCommandService(IBackOfficeRepository backOfficeRepository) : ICommandService<ImportLocalizationFileCommand>
{
    private readonly IBackOfficeRepository _backOfficeRepository = backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));

    public ValueTask ExecuteAsync(ImportLocalizationFileCommand command, CancellationToken cancellationToken = default)
        => _backOfficeRepository.ImportLocalizationStringsAsync(
            command.SystemCode,
            ReadLocalizationFileAsync(
                command.SystemCode,
                command.FileStream).ToEnumerable());

    private static async IAsyncEnumerable<LocalizationStringInfo> ReadLocalizationFileAsync(string systemCode, Stream fileStream)
    {
        using var reader = new XLWorkbook(fileStream, new LoadOptions
        {
            GraphicEngine = new DefaultGraphicEngine("DejaVu Sans")
        });

        foreach (var sheet in reader.Worksheets)
        {
            if (!TryGetCultureInfo(sheet.Name, out var cultureInfo))
                continue;

            var cultureCode = cultureInfo.Name;

            var index = 2;
            var messageId = sheet.Cell(index, 1).GetFormattedString();

            while (!string.IsNullOrWhiteSpace(messageId))
            {
                var text = sheet.Cell(index, 2).GetFormattedString();

                yield return new LocalizationStringInfo(systemCode, cultureCode, messageId, text);

                messageId = sheet.Cell(++index, 1).GetFormattedString();
            }
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    private static bool TryGetCultureInfo(string cultureCode, out CultureInfo cultureInfo)
    {
        try
        {
            cultureInfo = CultureInfo.GetCultureInfo(cultureCode);
            return true;
        }
        catch
        {
            cultureInfo = null;
            return false;
        }
    }
}
