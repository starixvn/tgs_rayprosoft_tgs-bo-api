using System;
using System.Text.Json;

namespace TGS.Services.BackOffice.Application.Commands.BrandLiveChat;

public class CreateBrandLiveChatCommand
{
    public Guid BrandId { get; init; }

    public string Name { get; init; }

    public JsonDocument Setting { get; init; }
}
