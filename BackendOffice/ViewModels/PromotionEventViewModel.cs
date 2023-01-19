#nullable enable

using System;
using Tgs.Promotion.Clients;

namespace TGS.Apis.BackendOffice.ViewModels;

public class PromotionEventViewModel
{
    public PromotionEventAction Action { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public Guid EventId { get; set; }

    public string? Modifier { get; set; }

    public Guid PromotionId { get; set; }
}
