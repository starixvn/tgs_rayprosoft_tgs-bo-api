using System;
using System.Collections.Generic;
using System.Text.Json;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

public record UpdatePromotionRuleCommand(
    Guid PromotionId,
    JsonDocument Rule,
    IEnumerable<int> VipLevels,
    string Modifier);