using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using ClosedXML.Graphics;
using ClosedXML.Report;
using Microsoft.Extensions.FileProviders;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Localization;

internal class ExportLocalizationFileCommandService : IGetService<ExportLocalizationFileCommand, Stream>
{
    private static readonly EmbeddedFileProvider _EmbeddedFileProvider =
        new(Assembly.GetExecutingAssembly());

    private readonly IBackOfficeRepository _backOfficeRepository;
    private readonly IFileInfo _templateFileInfo;

    public ExportLocalizationFileCommandService(IBackOfficeRepository backOfficeRepository)
    {
        _backOfficeRepository = backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));
        _templateFileInfo = _EmbeddedFileProvider.GetFileInfo("/Assets/localization_template.xlsx");
    }

    public async ValueTask<Stream> GetAsync(ExportLocalizationFileCommand query, CancellationToken cancellationToken = default)
    {
        var locStrings = from loc in _backOfficeRepository.ExportLocalizationStringsAsync(query.SystemCode)
                         group loc by loc.CultureCode into g
                         select new
                         {
                             CultureCode = g.Key,
                             LocalizationDict = g.ToEnumerable()
                                .ToDictionary(l => l.MessageId, l => l.Text)
                         };

        using var workbook = new XLWorkbook(new LoadOptions
        {
            GraphicEngine = new DefaultGraphicEngine("DejaVu Sans")
        });

        await foreach (var g in locStrings)
            GenerateLanguageWorkSheet(workbook, g.CultureCode, g.LocalizationDict);

        var ms = new MemoryStream();

        if (workbook.Worksheets.Count == 0)
            GenerateEmptyWorkSheet(workbook);

        workbook.SaveAs(ms);
        ms.Position = 0;

        return ms;
    }

    private void GenerateEmptyWorkSheet(XLWorkbook target)
    {
        using var fs = _templateFileInfo.CreateReadStream();
        using var templateWorkbook = new XLWorkbook(
            fs,
            new LoadOptions
            {
                GraphicEngine = new DefaultGraphicEngine("DejaVu Sans")
            });
        using var template = new XLTemplate(templateWorkbook);

        template.AddVariable(new
        {
            Rows = Array.Empty<object>()
        });

        _ = template.Generate();

        _ = template.Workbook.Worksheet("template").CopyTo(target, "en-US");
    }

    private void GenerateLanguageWorkSheet(
        XLWorkbook target,
        string cultureCode,
        Dictionary<string, string> locDict)
    {
        using var fs = _templateFileInfo.CreateReadStream();
        using var templateWorkbook = new XLWorkbook(
            fs,
            new LoadOptions
            {
                GraphicEngine = new DefaultGraphicEngine("DejaVu Sans")
            });
        using var template = new XLTemplate(templateWorkbook);

        template.AddVariable(new
        {
            Rows = locDict.Select(p => new
            {
                MessageId = p.Key,
                Text = p.Value
            })
        });

        _ = template.Generate();

        _ = template.Workbook.Worksheet("template").CopyTo(target, cultureCode);
    }
}
