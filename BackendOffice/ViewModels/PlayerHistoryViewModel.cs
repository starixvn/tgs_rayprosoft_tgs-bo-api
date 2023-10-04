#nullable enable

using System;
using Tgs.PlayerService.Client;

namespace TGS.Apis.BackendOffice.ViewModels;

public class PlayerHistoryViewModel
{
    public EventAction Action { get; set; } = default!;

    public DateTimeOffset CreatedOn { get; set; }

    public Guid EventId { get; set; }

    public string? Modifier { get; set; }
}
