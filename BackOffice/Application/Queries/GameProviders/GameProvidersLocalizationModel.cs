using System;
using System.Collections.Generic;
using System.Text.Json;

namespace TGS.Services.BackOffice.Application.Queries.GameProviders;

public class GameProvidersLocalizationModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public IDictionary<string, JsonDocument> Localization { get; set; }
}