using System;

namespace TGS.Services.BackOffice.ApplicationServices;

public record AddAgentDomainCommand(Guid AgentId, string DomainName);
