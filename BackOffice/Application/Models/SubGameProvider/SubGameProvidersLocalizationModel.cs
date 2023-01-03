using System.Collections.Generic;
using System.Text.Json;

namespace TGS.Services.BackOffice.Application.Models.SubGameProvider;

public class SubGameProvidersLocalizationModel
{
    public string Code { get; set; }

    public IDictionary<string, JsonDocument> Localization { get; set; }

    public string Name { get; set; }
}
