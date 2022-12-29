namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandLiveChat;

public record UpdateBrandLiveChatRequest
{
    public string Name { get; init; }
    public string Setting { get; init; }
}
