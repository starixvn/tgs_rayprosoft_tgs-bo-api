#nullable enable

using System;

namespace TGS.Apis.BackendOffice.ViewModels;

public class PromotionMainInfoViewModel
{
    public bool AutoSentPlayerBonus { get; init; }

    public Guid BrandId { get; init; }

    public DateTimeOffset From { get; init; }

    public Guid Id { get; init; }

    public string Name { get; init; } = default!;

    public string PromotionType { get; init; } = default!;

    public DateTimeOffset To { get; init; }

    public decimal? TotalAmountLimit { get; init; }

    public bool Unlimited { get; init; }
}
