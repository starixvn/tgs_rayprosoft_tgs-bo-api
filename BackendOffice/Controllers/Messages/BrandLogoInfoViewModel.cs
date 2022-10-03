using System;
using System.Text.Json.Serialization;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages;

public class BrandLogoInfoViewModel
{
    public Guid BrandId { get; init; }

    [JsonPropertyName("desktop")]
    public Uri DesktopLogoUrl { get; init; }

    [JsonPropertyName("loading")]
    public Uri LoadingIconUrl { get; init; }

    [JsonPropertyName("mobile")]
    public Uri MobileLogoUrl { get; init; }
}
