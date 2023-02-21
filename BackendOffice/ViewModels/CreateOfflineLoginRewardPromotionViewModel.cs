#nullable enable

using System;

namespace TGS.Apis.BackendOffice.ViewModels;

public class CreateOfflineLoginRewardPromotionViewModel
{
    public Guid BatchId { get; init; } = default!;

    public decimal BonusAmount { get; init; }

    public DateOnly From { get; init; } = default!;

    public string Name { get; init; } = default!;

    public DateOnly To { get; init; } = default!;

    public decimal TurnoverMultiple { get; init; }
}