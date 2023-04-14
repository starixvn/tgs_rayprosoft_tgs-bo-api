#nullable enable

using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

public record PromotionListQuery(
    Guid BrandId, 
    string? Name,
    DateOnly? From,
    DateOnly? To,
    SwitchStatus? Status,
    string? PromotionType);