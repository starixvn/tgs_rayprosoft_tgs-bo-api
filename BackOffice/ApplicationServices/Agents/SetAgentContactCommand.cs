# nullable enable

using System;
using System.Collections.Generic;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

public record SetAgentContactCommand(
    Guid AgentId,
    IDictionary<string, string?> ModifyContact,
    string ModifyUserId,
    string ModifyUsername);
