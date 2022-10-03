#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TGS.Services.BackOffice.Domain;

public interface IAgentRepository
{
    ValueTask AddDomainToAgentAsync(Guid agentId, string domainName);

    ValueTask<AgentInfo?> GetAgentAsync(Guid brandId, string userName, CancellationToken cancellationToken = default);

    IAsyncEnumerable<AgentInfo> GetAgentsAsync(Guid brandId);

    IAsyncEnumerable<string> QueryDomainsFromAgentAsync(Guid agentId);

    ValueTask RemoveDomainFromAgentAsync(Guid agentId, string domainName);
}
