using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Payment;

internal class PlayerPaymentStatisticsDetailQueryService : IQueryService<PlayerPaymentStatisticsDetailQuery, PlayerDailyPaymentItem>
{
    private readonly IPaymentReportingRepository _paymentReportingRepository;
    private readonly ISystemClock _systemClock;

    public PlayerPaymentStatisticsDetailQueryService(IPaymentReportingRepository paymentReportingRepository, ISystemClock systemClock)
    {
        _paymentReportingRepository = paymentReportingRepository ?? throw new ArgumentNullException(nameof(paymentReportingRepository));
        _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
    }

    public IAsyncEnumerable<PlayerDailyPaymentItem> QueryAsync(
        PlayerPaymentStatisticsDetailQuery query,
        CancellationToken cancellationToken = default)
    {
        var currentDate = DateOnly.FromDateTime(_systemClock.UtcNow.DateTime);

        return query.QueryDate >= currentDate
            ? _paymentReportingRepository.QueryRealTimePlayerDailyDepositStatisticsAsync(
                query.BrandId,
                query.QueryDate,
                cancellationToken)
            : _paymentReportingRepository.QueryPlayerDailyDepositStatisticsAsync(
                    query.BrandId,
                    query.QueryDate,
                    cancellationToken);
    }
}
