using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Tgs.MarketingReport.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices.Agents;

namespace TGS.Services.BackOffice.Cache;

internal class CachedAgentPlayerRegisterDetailQueryService : IQueryService<AgentPlayerRegisterDetailQuery, PlayerRegisterDetail>
{
    private readonly IMemoryCache _memoryCache;
    private readonly IQueryService<AgentPlayerRegisterDetailQuery, PlayerRegisterDetail> _sourceService;

    public CachedAgentPlayerRegisterDetailQueryService(IQueryService<AgentPlayerRegisterDetailQuery, PlayerRegisterDetail> sourceService, IMemoryCache memoryCache)
    {
        _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async IAsyncEnumerable<PlayerRegisterDetail> QueryAsync(
        AgentPlayerRegisterDetailQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var key = $"PlayerDailyRegisterItem:{query.BrandId}:{query.ReportDate}";

        var result = await _memoryCache.GetOrCreateAsync(
            key,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1D);

                return await _sourceService
                    .QueryAsync(query, cancellationToken)
                    .ToArrayAsync()
                    .ConfigureAwait(false);
            }).ConfigureAwait(false);

        foreach (var item in result!)
            yield return item;
    }
}
