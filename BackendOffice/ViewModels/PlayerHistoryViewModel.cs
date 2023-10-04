#nullable enable

using System;

namespace TGS.Apis.BackendOffice.ViewModels;

public class PlayerHistoryViewModel
{
    public string Action { get; set; } = default!;

    public DateTimeOffset CreatedOn { get; set; }

    public Guid EventId { get; set; }

    public string? Modifier { get; set; }
}
