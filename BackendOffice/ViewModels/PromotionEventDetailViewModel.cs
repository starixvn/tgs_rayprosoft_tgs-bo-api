#nullable enable

using System;
using System.Text.Json;
using Tgs.Promotion.Clients;

namespace TGS.Apis.BackendOffice.ViewModels;

public class PromotionEventDetailViewModel
{
    public Guid EventId { get; init; }

    public Guid PromotionId { get; init; }

    public PromotionEventAction Action { get; init; }

    public JsonDocument ChangeSets { get; init; } = default!;

    public DateTimeOffset CreatedOn { get; init; }

    public string? Modifier { get; init; }
}