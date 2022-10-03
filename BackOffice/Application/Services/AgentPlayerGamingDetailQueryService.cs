using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.MarketingReport.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Services;

internal class AgentPlayerGamingDetailQueryService : IQueryService<AgentPlayerGamingDetailQuery, PlayerGamingDetail>
{
    private readonly IMarketingReportClient _marketingReportClient;

    public AgentPlayerGamingDetailQueryService(IMarketingReportClient marketingReportClient)
    {
        _marketingReportClient = marketingReportClient ?? throw new ArgumentNullException(nameof(marketingReportClient));
    }

    public IAsyncEnumerable<PlayerGamingDetail> QueryAsync(AgentPlayerGamingDetailQuery query, CancellationToken cancellationToken = default)
        => _marketingReportClient.QueryPlayerGamingDetailAsync(query.BrandId, query.ReportDate, cancellationToken);
}
