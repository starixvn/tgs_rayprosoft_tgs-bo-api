using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.MarketingReport.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Services;

internal class AgentMarketingReportQueryService : IQueryService<AgentMarketingReportQuery, DailyAggregateInfo>
{
    private readonly IMarketingReportClient _marketingReportClient;

    public AgentMarketingReportQueryService(IMarketingReportClient marketingReportClient)
    {
        _marketingReportClient = marketingReportClient ?? throw new ArgumentNullException(nameof(marketingReportClient));
    }

    public IAsyncEnumerable<DailyAggregateInfo> QueryAsync(AgentMarketingReportQuery query, CancellationToken cancellationToken = default)
    {
        var currentDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow
            .ToOffset(TimeSpan.FromHours(8D))
            .DateTime);

        return _marketingReportClient.QueryMarketingStatisticsAsync(query.BrandId, currentDate);
    }
}
