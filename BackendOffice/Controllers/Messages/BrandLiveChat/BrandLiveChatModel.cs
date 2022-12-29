using System.Text.Json;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandLiveChat;

public record BrandLiveChatModel
{
    public string Name { get; init; }
    public JsonDocument Setting { get; set; }
}
