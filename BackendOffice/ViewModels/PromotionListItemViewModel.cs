#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.ViewModels;

public class PromotionListItemViewModel
{
    public decimal AmountUsed { get; init; }

    public DateTime From { get; init; }

    public Guid Id { get; init; }

    public string Name { get; init; } = default!;

    public int Number { get; init; }

    public string PromotionType { get; init; } = default!;

    public SwitchStatus Status { get; init; }

    public DateTime To { get; init; }

    public decimal? TotalAmountLimit { get; init; }

    public bool Unlimited { get; init; }

    public IEnumerable<int> VipLevels { get; init; } = Enumerable.Empty<int>();
}
