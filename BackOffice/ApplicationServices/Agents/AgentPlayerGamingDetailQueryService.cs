using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.MarketingReport.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class AgentPlayerGamingDetailQueryService(IMarketingReportClient marketingReportClient) : IQueryService<AgentPlayerGamingDetailQuery, PlayerGamingDetail>
{
    private readonly IMarketingReportClient _marketingReportClient = marketingReportClient ?? throw new ArgumentNullException(nameof(marketingReportClient));

    public IAsyncEnumerable<PlayerGamingDetail> QueryAsync(AgentPlayerGamingDetailQuery query, CancellationToken cancellationToken = default)
        => _marketingReportClient.QueryPlayerGamingDetailAsync(query.BrandId, query.ReportDate, cancellationToken);
}
