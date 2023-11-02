#nullable enable

using System;
using Tgs.Agent.Client.Models.Enums;

namespace TGS.Apis.BackendOffice.ViewModels;

public class AgentHistoryViewModel
{
    public EventAction Action { get; set; } = default!;

    public DateTimeOffset CreatedOn { get; set; }

    public Guid EventId { get; set; }

    public string? Modifier { get; set; }
}
