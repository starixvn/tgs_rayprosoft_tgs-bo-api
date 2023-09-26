#nullable enable

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.ViewModels;

public class PromotionListQueryViewModel
{
    public Guid BrandId { get; init; }

    public DateTimeOffset? From { get; init; }

    public string? Name { get; init; }

    [FromQuery(Name = "promotionType")]
    public IEnumerable<string>? PromotionTypes { get; init; }

    public SwitchStatus? Status { get; init; }

    public DateTimeOffset? To { get; init; }
}
