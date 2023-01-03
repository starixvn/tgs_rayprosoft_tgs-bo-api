using System;
using System.Text.Json;

namespace TGS.Services.BackOffice.Application.Models.SubGameProvider;

public record SubGameProviderRequest
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid GameProviderId { get; set; }
    public string EditorUsername { get; set; }
    public JsonDocument Localization { get; set; }
}
