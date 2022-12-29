using System.Text.Json;

namespace TGS.Services.BackOffice.Application.Queries.BrandLiveChat;

public class BrandLiveChatInfo
{
    public string ChatName { get; init; }

    public JsonDocument ChatSetting { get; init; }
}
