#nullable enable

using System;

namespace TGS.Apis.BackendOffice.ViewModels;

public class PromotionMainInfoViewModel
{
    public Guid Id { get; init; }

    public Guid BrandId { get; init; }

    public string Name { get; init; } = default!;

    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }

    public string PromotionType { get; init; } = default!;

    public decimal? PromotionLimitAmount { get; init; }

    public bool AutoSentPlayerBonus { get; init; }
}