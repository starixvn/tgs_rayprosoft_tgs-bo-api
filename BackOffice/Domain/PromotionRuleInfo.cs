#nullable enable

using System.Text.Json;

namespace TGS.Services.BackOffice.Domain;
public record PromotionRuleInfo(
    JsonDocument CompaignDetail);
