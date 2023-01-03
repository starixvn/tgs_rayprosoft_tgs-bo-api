using System;
using System.Text.Json;

#nullable enable

namespace TGS.Apis.BackendOffice.Controllers.Messages.Promotion;

public class PromotionViewModel
{
    public Guid Id { get; init; }

    public JsonDocument Localization { get; init; } = default!;

    public string Name { get; init; } = default!;
}
