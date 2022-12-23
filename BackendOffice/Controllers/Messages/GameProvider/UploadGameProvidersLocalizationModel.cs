using System.Text.Json.Serialization;

namespace TGS.Apis.BackendOffice.Controllers.Messages.GameProvider;

public class UploadGameProvidersLocalizationModel
{
    [JsonPropertyName("enUS")]
    public string EnUS { get; set; }

    public string Id { get; set; }

    [JsonPropertyName("idID")]
    public string IdID { get; set; }

    [JsonPropertyName("jaJP")]
    public string JaJP { get; set; }

    [JsonPropertyName("koKR")]
    public string KoKR { get; set; }

    public string Name { get; set; }

    [JsonPropertyName("thTH")]
    public string ThTH { get; set; }

    [JsonPropertyName("viVN")]
    public string ViVN { get; set; }

    [JsonPropertyName("zhCN")]
    public string ZhCN { get; set; }

    [JsonPropertyName("zhTW")]
    public string ZhTW { get; set; }
}
