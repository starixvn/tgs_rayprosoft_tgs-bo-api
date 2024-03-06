using System;
using System.Collections.Generic;

namespace TGS.Apis.BackendOffice.ViewModels;

public class CreateOfflineDepositRewardPromotionViewModel
{
    public Guid BatchId { get; init; } = default!;

    public DateOnly From { get; init; } = default!;

    public string Name { get; init; } = default!;

    public IEnumerable<OfflineDepositPhaseViewModel> Phases { get; set; } = [];

    public DateOnly To { get; init; } = default!;

    public decimal TurnoverMultiple { get; init; }
}
