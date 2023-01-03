using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandLiveChat;

public record CreateBrandLiveChatRequest
{
    public Guid BrandId { get; init; }
    public string Name { get; init; }
    public string Setting { get; init; }
}
