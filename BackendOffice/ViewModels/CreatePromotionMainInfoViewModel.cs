using System;

namespace TGS.Apis.BackendOffice.ViewModels;

public class CreatePromotionMainInfoViewModel
{
    public bool AutoSentPlayerBonus { get; init; }

    public DateOnly From { get; init; }

    public string Name { get; init; } = default!;

    public string PromotionType { get; init; } = default!;

    public DateOnly To { get; init; }

    public decimal? TotalAmountLimit { get; init; }

    public bool Unlimited { get; init; }
}