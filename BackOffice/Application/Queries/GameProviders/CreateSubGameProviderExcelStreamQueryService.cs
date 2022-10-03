using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.GameProviders;

internal class CreateSubGameProviderExcelStreamQueryService : IGetService<CreateSubGameProviderExcelStreamQuery, Stream>
{
    public ValueTask<Stream> GetAsync(CreateSubGameProviderExcelStreamQuery query, CancellationToken cancellationToken = default)
    {
        var memoryStream = new MemoryStream();

        using (var document = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());

            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "SubGameProvider"
            });

            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            var row = new Row();
            row.Append(
                new Cell
                {
                    CellValue = new CellValue("Id"),
                    DataType = CellValues.String
                },
                new Cell
                {
                    CellValue = new CellValue("Name"),
                    DataType = CellValues.String
                });

            foreach (var item in query.Languages)
            {
                row.Append(
                    new Cell
                    {
                        CellValue = new CellValue(item),
                        DataType = CellValues.String
                    });
            }

            sheetData.AppendChild(row);

            foreach (var gameProvider in query.SubGameProviders)
            {
                row = new Row();
                row.Append(
                    new Cell
                    {
                        CellValue = new CellValue(gameProvider.SubGameProviderCode),
                        DataType = CellValues.String
                    },
                    new Cell
                    {
                        CellValue = new CellValue(gameProvider.SubGameProviderName),
                        DataType = CellValues.String
                    }
                );

                if (gameProvider.Localization != null)
                {
                    foreach (var item in query.Languages)
                    {
                        var localization = JsonDocument.Parse(gameProvider.Localization);
                        JsonElement lang;
                        var isExist = localization.RootElement.TryGetProperty(item, out lang);
                        if (isExist)
                        {
                            row.Append(
                                new Cell
                                {
                                    CellValue = new CellValue(lang.GetProperty("name").ToString()),
                                    DataType = CellValues.String
                                }
                            );
                        }
                    }
                }

                sheetData.AppendChild(row);
            }
        }

        memoryStream.Seek(0, SeekOrigin.Begin);

        return ValueTask.FromResult<Stream>(memoryStream);
    }
}
