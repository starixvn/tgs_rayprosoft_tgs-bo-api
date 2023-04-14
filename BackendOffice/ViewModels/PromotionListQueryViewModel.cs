using System;
using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.ViewModels;

public class PromotionListQueryViewModel
{
    public Guid BrandId { get; set; }

    public DateOnly? From { get; set; }

    public string Name { get; set; }

    public SwitchStatus? Status { get; set; }

    public DateOnly? To { get; set; }

    public string PromotionType { get; set; }
}