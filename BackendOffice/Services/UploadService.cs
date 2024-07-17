using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;

namespace TGS.Apis.BackendOffice.Services;

public interface IUploadService
{
    Task<List<T>> GetUploadedExcelAsync<T>(IFormFile file) where T : new();
}

public class UploadService : IUploadService
{
    async Task<List<T>> IUploadService.GetUploadedExcelAsync<T>(IFormFile file)
    {
        var result = new List<T>();

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        using var reader = ExcelReaderFactory.CreateReader(stream);

        _ = reader.Read();
        var headers = new List<string>();
        for (var i = 0; i < reader.FieldCount; i++)
            headers.Add(reader.GetValue(i).ToString());
        var fieldCount = reader.FieldCount;
        while (reader.Read()) //Each row of the file
        {
            var obj = new ExpandoObject();

            for (var i = 0; i < fieldCount; i++)
            {
                var header = headers[i];
                _ = obj.TryAdd(header, reader.GetValue(i));

                ((IDictionary<string, object>)obj)[header] = reader.GetValue(i).ToString();
            }

            var json = JsonSerializer.Serialize(obj);
            result.Add(JsonSerializer.Deserialize<T>(json));
        }

        return result;
    }
}
