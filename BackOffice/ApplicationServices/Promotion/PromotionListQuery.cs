#nullable enable

using System;
using System.Collections.Generic;
using Tgs.Promotion.Clients;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

public record PromotionListQuery(
    Guid BrandId,
    string? Name,
    During? EventDuring,
    SwitchStatus? Status,
    IEnumerable<string>? PromotionTypes);
