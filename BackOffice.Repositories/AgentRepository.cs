#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.BackOffice.Domain;

namespace TGS.BackOffice.Repositories;

internal class AgentRepository : IAgentRepository
{
    private readonly ITgsRepository _tgsRepository;

    public AgentRepository(ITgsRepository tgsRepository)
    {
        _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
    }

    public async ValueTask AddDomainToAgentAsync(Guid agentId, string domainName)
    {
        await _tgsRepository.AgentDomains
            .AddAsync(new AgentDomain
            {
                AgentId = agentId,
                DomainName = domainName
            })
            .ConfigureAwait(false);

        await _tgsRepository
            .SaveChangesAsync()
            .ConfigureAwait(false);
    }

    public async ValueTask<AgentInfo?> GetAgentAsync(Guid brandId, string userName, CancellationToken cancellationToken = default)
        => await _tgsRepository.Agents
            .Where(agent => agent.BrandId == brandId && agent.Username == userName)
            .Select(agent => new AgentInfo(
                agent.Id,
                agent.Username,
                agent.DefaultWalletId,
                agent.Status))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

    public IAsyncEnumerable<AgentInfo> GetAgentsAsync(Guid brandId)
        => _tgsRepository.Agents.AsQueryable()
            .Where(agent => agent.BrandId == brandId)
            .Select(agent => new AgentInfo(
                agent.Id,
                agent.Username,
                agent.DefaultWalletId,
                agent.Status))
            .ToAsyncEnumerable();

    public IAsyncEnumerable<string> QueryDomainsFromAgentAsync(Guid agentId)
        => _tgsRepository.AgentDomains.AsQueryable()
            .Where(domain => domain.AgentId == agentId)
            .Select(domain => domain.DomainName)
            .ToAsyncEnumerable();

    public async ValueTask RemoveDomainFromAgentAsync(Guid agentId, string domainName)
    {
        var domainData = await _tgsRepository.AgentDomains
            .FindAsync(agentId, domainName)
            .ConfigureAwait(false);

        _tgsRepository.AgentDomains.Remove(domainData!);

        await _tgsRepository.SaveChangesAsync().ConfigureAwait(false);
    }
}
