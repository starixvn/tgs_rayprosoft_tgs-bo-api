using System;

namespace TGS.Services.BackOffice.Application.Services;

public record AddAgentDomainCommand(Guid AgentId, string DomainName);
