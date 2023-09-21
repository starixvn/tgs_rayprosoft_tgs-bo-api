using System.Text.Json.Serialization;

namespace TGS.Services.BackOffice.ApplicationServices.Overview;

public class GameNameLocalization
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}
