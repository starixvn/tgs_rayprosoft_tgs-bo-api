# nullable enable

using System;
using System.Collections.Generic;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

public record SetAgentInfoCommand(
    Guid AgentId,
    IDictionary<string, string?> ModifyInfo,
    string ModifyUserId,
    string ModifyUsername);
