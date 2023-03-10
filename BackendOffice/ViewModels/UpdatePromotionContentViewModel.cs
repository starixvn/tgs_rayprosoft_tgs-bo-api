#nullable enable

using System.Text.Json;

namespace TGS.Apis.BackendOffice.ViewModels;

public class UpdatePromotionContentViewModel
{
    public string? Content { get; set; }
    public string? Terms { get; set; }
    public JsonDocument? Localization { get; set; } = default!;
    public string? MobileBanner { get; set; }
    public string? DesktopBanner { get; set; }
    public string? SubSubject { get; set; }
}