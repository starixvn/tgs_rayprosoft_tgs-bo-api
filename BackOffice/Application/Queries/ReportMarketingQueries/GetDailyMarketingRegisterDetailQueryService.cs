using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Queries.ReportMarketingQueries;

internal class GetDailyMarketingRegisterDetailQueryService(IMarketingReportRepository marketingReportRepository) : IQueryService<GetDailyMarketingRegisterDetailQuery, PlayerDailyRegisterItem>
{
    private readonly IMarketingReportRepository _marketingReportRepository = marketingReportRepository ?? throw new ArgumentNullException(nameof(marketingReportRepository));

    public IAsyncEnumerable<PlayerDailyRegisterItem> QueryAsync(
        GetDailyMarketingRegisterDetailQuery query,
        CancellationToken cancellationToken = default)
        => _marketingReportRepository.QueryRealTimePlayerDailyRegisterStatisticsAsync(
            query.BrandId,
            query.QueryDate,
            cancellationToken);
}
