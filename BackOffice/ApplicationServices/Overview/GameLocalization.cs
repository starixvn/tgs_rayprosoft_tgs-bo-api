using System.Text.Json.Serialization;

namespace TGS.Services.BackOffice.ApplicationServices.Overview;

public class GameLocalization
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}
