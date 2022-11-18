#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.DepositAccumulateRoulette.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class DepositAccumulateRouletteGameRecordQueryService
    : IQueryService<DepositAccumulateRouletteGameRecordQuery, RouletteRecord>
{
    private readonly IDepositAccumulateRouletteServiceClient _rouletteServiceClient;
    private readonly ISystemClock _systemClock;

    public DepositAccumulateRouletteGameRecordQueryService(
        ISystemClock systemClock,
        IDepositAccumulateRouletteServiceClient rouletteServiceClient)
    {
        _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        _rouletteServiceClient = rouletteServiceClient ?? throw new ArgumentNullException(nameof(rouletteServiceClient));
    }

    public IAsyncEnumerable<RouletteRecord> QueryAsync(
        DepositAccumulateRouletteGameRecordQuery query,
        CancellationToken cancellationToken = default)
    {
        (DateTimeOffset Begin, DateTimeOffset End) timeRange;

        if (query.TimeRange is not null)
        {
            timeRange = (query.TimeRange.Value.Begin, query.TimeRange.Value.End);
        }
        else
        {
            var today = DateOnly.FromDateTime(_systemClock.UtcNow.DateTime);

            var utcTimeOnly = new TimeOnly(16, 0, 0);
            var end = new DateTimeOffset(today.ToDateTime(utcTimeOnly, DateTimeKind.Utc));
            var begin = new DateTimeOffset(today.AddDays(-3).ToDateTime(utcTimeOnly, DateTimeKind.Utc));
            timeRange = (begin, end);
        }

        return _rouletteServiceClient.GetRouletteRecordsAsync(
            query.BrandId,
            new RouletteRecordQuery
            {
                PlayerCodes = query.PlayerCodes,
                PromotionIds = query.PromotionIds,
                BeginTime = timeRange.Begin,
                EndTime = timeRange.End
            }, cancellationToken);
    }
}
