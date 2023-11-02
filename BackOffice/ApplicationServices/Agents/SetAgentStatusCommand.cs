using System;
using Tgs.Agent.Client.Models.Enums;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

public record SetAgentStatusCommand(
    Guid AgentId,
    AgentStatus Status,
    string ModifyUserId,
    string ModifyUsername);
