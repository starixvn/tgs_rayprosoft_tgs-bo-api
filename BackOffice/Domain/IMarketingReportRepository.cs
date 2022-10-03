using System;
using System.Collections.Generic;
using System.Threading;

namespace TGS.Services.BackOffice.Domain;

public interface IMarketingReportRepository
{
    IAsyncEnumerable<PlayerDailyRegisterItem> QueryRealTimePlayerDailyRegisterStatisticsAsync(
        Guid brandId,
        DateOnly queryDate,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<PlayerDailyGamingItem> QueryRealTimePlayerDailyGamingStatisticsAsync(
        Guid brandId,
        DateOnly queryDate,
        CancellationToken cancellationToken = default);
}
