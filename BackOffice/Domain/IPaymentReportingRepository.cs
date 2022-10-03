using System;
using System.Collections.Generic;
using System.Threading;

namespace TGS.Services.BackOffice.Domain;

public interface IPaymentReportingRepository
{
    IAsyncEnumerable<AgentPlayerDepositStatistics> QueryAgentDailyDepositStatisticsAsync(
        Guid brandId,
        DateOnly queryLatestDate,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<PlayerDailyPaymentItem> QueryPlayerDailyDepositStatisticsAsync(
        Guid brandId,
        DateOnly queryDate,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<PlayerDailyPaymentItem> QueryRealTimePlayerDailyDepositStatisticsAsync(
        Guid brandId,
        DateOnly queryDate,
        CancellationToken cancellationToken = default);
}
