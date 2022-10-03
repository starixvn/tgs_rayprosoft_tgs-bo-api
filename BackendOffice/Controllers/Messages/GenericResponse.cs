using System.Text.Json.Serialization;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages;

public class GenericResponse<TData> : MessageResponse
{
    [JsonPropertyName("data")]
    public TData Data { get; set; }
}
