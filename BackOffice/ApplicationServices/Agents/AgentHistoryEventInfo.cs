# nullable enable

using System;
using System.Text.Json;
using Tgs.Agent.Client.Models.Enums;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

public record AgentHistoryEventInfo(
    EventAction Action,
    JsonDocument ChangeSets,
    DateTime CreatedOn,
    string? Modifier);
