using System.Text.Json.Serialization;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Game;

public class UploadGameLocalizationModel
{
    public string Id { get; set; }

    public string Code { get; set; }

    [JsonPropertyName("enUS")]
    public string EnUS { get; set; }

    [JsonPropertyName("zhCN")]
    public string ZhCN { get; set; }

    [JsonPropertyName("zhTW")]
    public string ZhTW { get; set; }

    [JsonPropertyName("thTH")]
    public string ThTH { get; set; }

    [JsonPropertyName("jaJP")]
    public string JaJP { get; set; }

    [JsonPropertyName("koKR")]
    public string KoKR { get; set; }

    [JsonPropertyName("idID")]
    public string IdID { get; set; }

    [JsonPropertyName("viVN")]
    public string ViVN { get; set; }
}