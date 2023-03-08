using System.Text.Json;
using System.Text.Json.Serialization;

namespace TGS.Services.BackOffice.Domain;

public static class BackOfficeUtility
{
    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };
}
