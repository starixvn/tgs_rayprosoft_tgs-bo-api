using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Tgs.MarketingReport.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices.Agents;

namespace TGS.Services.BackOffice.Cache;

internal class CachedAgentMarketingReportQueryService(IQueryService<AgentMarketingReportQuery, DailyAggregateInfo> sourceService, IMemoryCache memoryCache) : IQueryService<AgentMarketingReportQuery, DailyAggregateInfo>
{
    private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    private readonly IQueryService<AgentMarketingReportQuery, DailyAggregateInfo> _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));

    public async IAsyncEnumerable<DailyAggregateInfo> QueryAsync(
        AgentMarketingReportQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.Now;
        var recordDate = DateOnly.FromDateTime(now.ToUniversalTime().DateTime);

        var key = $"AgentMarketingReport:{query.BrandId}:{recordDate}";

        var result = await _memoryCache.GetOrCreateAsync(
            key,
            async entry =>
            {
                entry.AbsoluteExpiration = new DateTimeOffset(
                    now.ToUniversalTime().Date.AddDays(1D).AddHours(1D),
                    TimeSpan.Zero);

                return await _sourceService
                    .QueryAsync(query, cancellationToken)
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);
            }).ConfigureAwait(false);

        foreach (var item in result!)
            yield return item;
    }
}
