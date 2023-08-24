using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ClosedXML.Excel;
using ClosedXML.Graphics;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;

internal class ReadOfflinePromotionUserListQueryService : IQueryService<ReadOfflinePromotionUserListQuery, string>
{
    public IAsyncEnumerable<string> QueryAsync(
        ReadOfflinePromotionUserListQuery query,
        CancellationToken cancellationToken = default)
    {
        using var reader = new XLWorkbook(
            query.ImportFile,
            new LoadOptions
            {
                GraphicEngine = new DefaultGraphicEngine("DejaVu Sans")
            });

        var sheet = reader.Worksheet(1);

        var index = 2;
        var name = sheet.Cell(index, 1).GetFormattedString();

        var userNames = new List<string>();

        while (!string.IsNullOrWhiteSpace(name))
        {
            userNames.Add(name);

            name = sheet.Cell(++index, 1).GetFormattedString();
        }

        return userNames.ToAsyncEnumerable();
    }
}
