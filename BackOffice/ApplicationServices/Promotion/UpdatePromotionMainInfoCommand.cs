using System;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

public record UpdatePromotionMainInfoCommand(
    Guid PromotionId,
    string Name,
    string PromotionType,
    DateOnly From,
    DateOnly To,
    bool Unlimited,
    decimal? TotalAmountLimit,
    bool IsAutoSent,
    string Modifier);