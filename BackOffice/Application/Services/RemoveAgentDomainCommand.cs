using System;

namespace TGS.Services.BackOffice.Application.Services;

public record RemoveAgentDomainCommand(Guid AgentId, string DomainName);
