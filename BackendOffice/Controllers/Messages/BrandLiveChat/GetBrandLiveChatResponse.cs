using System.Text.Json.Serialization;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandLiveChat;

public class GetBrandLiveChatResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("data")]
    public BrandLiveChatModel Data { get; set; }
}