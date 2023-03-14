#nullable enable

using System;
using System.Text.Json;
using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Promotion;

public class UpdatePromotionRequest
{
    public bool AutoSentPlayerBonus { get; init; }

    public decimal BonusAmountLimit { get; init; }

    public string? Content { get; init; }

    public string? DesktopBanner { get; init; }

    public DateOnly From { get; init; }

    public Guid[]? GameProviderIds { get; init; }

    public GameType? GameType { get; init; }

    public Guid Id { get; init; }

    public JsonDocument? Localization { get; init; }

    public string? MobileBanner { get; init; }

    public string Name { get; init; } = default!;

    public decimal Percentage { get; init; }

    public string PromotionType { get; init; } = default!;

    public JsonDocument? Rule { get; init; }

    public SwitchStatus Status { get; init; }

    public string SubSubject { get; init; } = default!;

    public string? Terms { get; init; }

    public DateOnly To { get; init; }

    public decimal? TotalAmountLimit { get; init; }

    public decimal TurnoverMultiple { get; init; }

    public bool Unlimited { get; init; }

    public int[] VipLevels { get; init; } = default!;
}
