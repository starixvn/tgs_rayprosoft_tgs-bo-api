#nullable enable

using System.Text.Json;

namespace TGS.Apis.BackendOffice.ViewModels;

public class UpdatePromotionContentViewModel
{
    public string? Content { get; init; }

    public string? DesktopBanner { get; init; }

    public JsonDocument? Localization { get; init; } = default!;

    public string? MobileBanner { get; init; }

    public string? SubSubject { get; init; }

    public string? Terms { get; init; }
}