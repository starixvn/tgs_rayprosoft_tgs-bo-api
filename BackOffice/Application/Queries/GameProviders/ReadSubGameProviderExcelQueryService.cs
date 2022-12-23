using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ExcelDataReader;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.GameProviders;

internal class ReadSubGameProviderExcelQueryService
    : IQueryService<ReadSubGameProviderExcelQuery, UploadSubGameProvidersLocalizationModel>
{
    public IAsyncEnumerable<UploadSubGameProvidersLocalizationModel> QueryAsync(
        ReadSubGameProviderExcelQuery query,
        CancellationToken cancellationToken = default)
    {
        using var reader = ExcelReaderFactory.CreateReader(query.FileStream);

        var headers = new List<string>();

        _ = reader.Read();
        for (var i = 0; i < reader.FieldCount; i++)
            headers.Add(reader.GetValue(i).ToString());

        var rows = new List<UploadSubGameProvidersLocalizationModel>();

        var fieldCount = reader.FieldCount;
        while (reader.Read()) //Each row of the file
        {
            var row = new UploadSubGameProvidersLocalizationModel()
            {
                Localization = new Dictionary<string, string>()
            };

            for (var i = 0; i < fieldCount; i++)
            {
                var header = headers[i];
                var value = reader.GetValue(i);

                if (header == "Code")
                {
                    row.Code = value?.ToString();
                    continue;
                }

                if (header == "Name")
                {
                    row.Name = value?.ToString();
                    continue;
                }

                row.Localization.Add(header, value?.ToString());
            }

            rows.Add(row);
        }

        return rows.ToAsyncEnumerable();
    }
}
