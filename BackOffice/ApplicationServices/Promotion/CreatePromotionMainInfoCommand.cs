using System;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

public record CreatePromotionMainInfoCommand(
    Guid BrandId,
    string Name,
    string PromotionType,
    DateOnly From,
    DateOnly To,
    bool Unlimited,
    decimal? TotalAmountLimit,
    bool IsAutoSent,
    string Modifier);