using System;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

public record AddAgentDomainCommand(Guid AgentId, string DomainName);
