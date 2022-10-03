using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tgs.MarketingReport.Client;
using TGS.Services.BackOffice.Application.Services;
using Tgs.ServiceUtility;
using Microsoft.Extensions.Caching.Memory;
using System.Runtime.CompilerServices;

namespace TGS.Services.BackOffice.Cache;
internal class CachedAgentMarketingReportQueryService : IQueryService<AgentMarketingReportQuery, DailyAggregateInfo>
{
    private readonly IQueryService<AgentMarketingReportQuery, DailyAggregateInfo> _sourceService;
    private readonly IMemoryCache _memoryCache;

    public CachedAgentMarketingReportQueryService(IQueryService<AgentMarketingReportQuery, DailyAggregateInfo> sourceService, IMemoryCache memoryCache)
    {
        _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }
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
                    .ToArrayAsync()
                    .ConfigureAwait(false);
            }).ConfigureAwait(false);

        foreach (var item in result)
            yield return item;
    }
}
