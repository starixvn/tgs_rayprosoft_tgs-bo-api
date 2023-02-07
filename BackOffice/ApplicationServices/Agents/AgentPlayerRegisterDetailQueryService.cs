using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.MarketingReport.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class AgentPlayerRegisterDetailQueryService : IQueryService<AgentPlayerRegisterDetailQuery, PlayerRegisterDetail>
{
    private readonly IMarketingReportClient _marketingReportClient;

    public AgentPlayerRegisterDetailQueryService(IMarketingReportClient marketingReportClient)
    {
        _marketingReportClient = marketingReportClient ?? throw new ArgumentNullException(nameof(marketingReportClient));
    }

    public IAsyncEnumerable<PlayerRegisterDetail> QueryAsync(AgentPlayerRegisterDetailQuery query, CancellationToken cancellationToken = default)
        => _marketingReportClient.QueryPlayerRegisterDetailAsync(query.BrandId, query.ReportDate, cancellationToken);
}
