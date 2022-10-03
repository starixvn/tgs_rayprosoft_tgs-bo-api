using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Services;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Cache;

internal class CachedAllAgentsByBrandQueryService : IQueryService<AllAgentsByBrandQuery, AgentInfo>
{
    private readonly IMemoryCache _memoryCache;
    private readonly IQueryService<AllAgentsByBrandQuery, AgentInfo> _sourceService;

    public CachedAllAgentsByBrandQueryService(IQueryService<AllAgentsByBrandQuery, AgentInfo> sourceService, IMemoryCache memoryCache)
    {
        _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async IAsyncEnumerable<AgentInfo> QueryAsync(
        AllAgentsByBrandQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var key = $"Agents:{query.BrandId}";

        var result = await _memoryCache.GetOrCreateAsync(
            key,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1D);

                return await _sourceService
                    .QueryAsync(query, cancellationToken)
                    .ToArrayAsync()
                    .ConfigureAwait(false);
            }).ConfigureAwait(false);

        foreach (var item in result)
            yield return item;
    }
}
