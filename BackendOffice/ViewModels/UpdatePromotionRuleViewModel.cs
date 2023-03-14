#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;
using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.ViewModels;

public class UpdatePromotionRuleViewModel
{
    public IEnumerable<Guid>? GameProviderIds { get; init; }

    public GameType? GameType { get; init; }

    public JsonDocument? Rule { get; init; }

    public IEnumerable<int> VipLevels { get; init; } = default!;
}