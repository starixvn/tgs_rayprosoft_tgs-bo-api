using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using TGS.Core.Infrastructures.Json;

namespace TGS.Services.BackOffice.Infrastructure.Services;

public interface IUploadService
{
    Task<List<T>> GetUploadedExcelAsync<T>(IFormFile file) where T : new();
}

public class UploadService : IUploadService
{
    private readonly IJsonSerializationProvider _jsonSerializationProvider;

    public UploadService(IJsonSerializationProvider jsonSerializationProvider)
    {
        _jsonSerializationProvider = jsonSerializationProvider;
    }

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
        {
            var header = reader.GetValue(i)?.ToString();
            if (header != null)
            {
                headers.Add(header);
            }
        }

        var fieldCount = headers.Count;
        while (reader.Read()) //Each row of the file
        {
            var obj = new ExpandoObject();

            for (var i = 0; i < fieldCount; i++)
            {
                var header = headers[i];
                var value = reader.GetValue(i);
                if (value != null)
                {
                    var fieldType = reader.GetFieldType(i);
                    _ = fieldType == typeof(bool)
                        ? obj.TryAdd(header, value)
                        : obj.TryAdd(header, value.ToString());
                }
            }

            var json = _jsonSerializationProvider.SerializeToString(obj,
                new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });

            var model = _jsonSerializationProvider.DeserializeFromString<T>(json);

            var isNull = model.GetType().GetProperties().All(p =>
                p.GetValue(model) == null);

            if (!isNull)
            {
                result.Add(_jsonSerializationProvider.DeserializeFromString<T>(json));
            }
        }

        return result;
    }
}
