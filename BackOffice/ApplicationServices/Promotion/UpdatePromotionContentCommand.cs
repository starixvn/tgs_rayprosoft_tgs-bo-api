#nullable enable

using System;
using System.Text.Json;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

public record UpdatePromotionContentCommand(
    Guid PromotionId,
    string? SubSubject,
    string? Content,
    JsonDocument Localization,
    string? DesktopBanner,
    string? MobileBanner,
    string? Terms,
    string Modifier);