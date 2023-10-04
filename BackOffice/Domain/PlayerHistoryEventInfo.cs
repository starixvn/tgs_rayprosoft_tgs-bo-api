#nullable enable

using System;
using System.Text.Json;
using Tgs.PlayerService.Client;

namespace TGS.Services.BackOffice.Domain;

public record PlayerHistoryEventInfo(
    EventAction Action,
    JsonDocument ChangeSets,
    DateTime CreatedOn,
    string? Modifier);
