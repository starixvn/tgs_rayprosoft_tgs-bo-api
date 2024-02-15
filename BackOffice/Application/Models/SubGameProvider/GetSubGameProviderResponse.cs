using System;
using System.Text.Json;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.SubGameProvider;

public class GetSubGameProviderResponse : MessageResponse
{
    public string Code { get; init; }

    public string Description { get; init; }

    public Guid GameProviderId { get; init; }

    public JsonDocument Localization { get; init; }

    public string Name { get; init; }
}
