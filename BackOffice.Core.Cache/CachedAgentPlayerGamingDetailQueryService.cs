using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Tgs.MarketingReport.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices.Agents;

namespace TGS.Services.BackOffice.Cache;

internal class CachedAgentPlayerGamingDetailQueryService(
    IQueryService<AgentPlayerGamingDetailQuery, PlayerGamingDetail> sourceService,
    IMemoryCache memoryCache)
    : IQueryService<AgentPlayerGamingDetailQuery, PlayerGamingDetail>
{
    private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    private readonly IQueryService<AgentPlayerGamingDetailQuery, PlayerGamingDetail> _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));

    public async IAsyncEnumerable<PlayerGamingDetail> QueryAsync(
        AgentPlayerGamingDetailQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var key = $"PlayerDailyGamingItem:{query.BrandId}:{query.ReportDate}";

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
