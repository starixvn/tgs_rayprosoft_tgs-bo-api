using System;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

public record SetAgentPasswordCommand(
    Guid AgentId,
    string Password,
    string ModifyUserId,
    string ModifyUsername);
