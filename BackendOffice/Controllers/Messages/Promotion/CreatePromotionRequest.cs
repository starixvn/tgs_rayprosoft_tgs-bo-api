#nullable enable

using System;
using System.Text.Json;
using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Promotion;

public class CreatePromotionRequest
{
    public bool AutoSentPlayerBonus { get; set; }

    public decimal BonusAmountLimit { get; set; }

    public Guid BrandId { get; set; }

    public string? Content { get; set; }

    public string? DesktopBanner { get; set; }

    public string? EditorUsername { get; set; }

    public DateTimeOffset From { get; set; }

    public Guid[]? GameProviderIds { get; set; }

    public GameType? GameType { get; set; }

    public JsonDocument? Localization { get; set; }

    public string? MobileBanner { get; set; }

    public string Name { get; set; } = default!;

    public decimal Percentage { get; set; }

    public string PromotionType { get; set; } = default!;

    public JsonDocument? Rule { get; set; }

    public SwitchStatus Status { get; set; }

    public string SubSubject { get; set; } = default!;

    public string? Terms { get; set; }

    public DateTimeOffset To { get; set; }

    public decimal? TotalAmountLimit { get; set; }

    public decimal TurnoverMultiple { get; set; }

    public bool Unlimited { get; set; }

    public int[] VipLevels { get; set; } = default!;
}
