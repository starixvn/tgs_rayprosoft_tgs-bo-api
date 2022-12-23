using System.Collections.Generic;
using System.Text.Json;

namespace TGS.Services.BackOffice.Domain;

public class GameLocalizationModel
{
    public string Code { get; set; }

    public int Id { get; set; }

    public IDictionary<string, JsonDocument> Localization { get; set; }
}
