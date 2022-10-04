using System;

namespace TGS.Services.BackOffice.ApplicationServices;

public record RemoveAgentDomainCommand(Guid AgentId, string DomainName);
