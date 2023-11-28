using System;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

public record UpdateAgentPayPasswordCommand(
    Guid AgentId,
    string PayPassword,
    string EditUsername,
    string EditUserId);
