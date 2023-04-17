#nullable enable

using System;
using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.ViewModels;

public class PromotionListQueryViewModel
{
    public Guid BrandId { get; init; }

    public string? Name { get; init; }

    public SwitchStatus? Status { get; init; }

    public string? PromotionType { get; init; }

    public DateOnlyViewModel? From { get; init; }

    public DateOnlyViewModel? To { get; init; }
}