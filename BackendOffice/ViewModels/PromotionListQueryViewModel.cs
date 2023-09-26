#nullable enable

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.ViewModels;

public class PromotionListQueryViewModel
{
    public Guid BrandId { get; init; }

    [ModelBinder(typeof(DateTimeOffsetToDateOnlyModelBinder), Name = "from")]
    public DateOnly? From { get; init; }

    public string? Name { get; init; }

    [FromQuery(Name = "promotionType")]
    public IEnumerable<string>? PromotionTypes { get; init; }

    public SwitchStatus? Status { get; init; }

    [ModelBinder(typeof(DateTimeOffsetToDateOnlyModelBinder), Name = "to")]
    public DateOnly? To { get; init; }
}
