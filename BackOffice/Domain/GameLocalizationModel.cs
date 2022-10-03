using System.Collections.Generic;
using System.Text.Json;

namespace TGS.Services.BackOffice.Domain;

public class GameLocalizationModel
{
    public int Id { get; set; }

    public IDictionary<string, JsonDocument> Localization { get; set; }

    public string Code { get; set; }
}