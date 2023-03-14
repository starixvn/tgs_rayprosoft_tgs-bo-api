#nullable enable

using System.Collections.Generic;
using System.Text.Json;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Domain;

public record PromotionRuleInfo(
    JsonDocument CompaignDetail,
    decimal BonusAmountLimit,
    decimal Percentage,
    decimal TurnoverMultiple,
    GameType? GameType,
    IEnumerable<int> VipLevels);
