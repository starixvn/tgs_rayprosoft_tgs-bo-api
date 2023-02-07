#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.DepositAccumulateRoulette.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

internal class DepositAccumulateRouletteGameRecordQueryService
    : IQueryService<DepositAccumulateRouletteGameRecordQuery, RouletteRecord>
{
    private readonly IDepositAccumulateRouletteServiceClient _rouletteServiceClient;

    public DepositAccumulateRouletteGameRecordQueryService(
        IDepositAccumulateRouletteServiceClient rouletteServiceClient)
    {
        _rouletteServiceClient = rouletteServiceClient ?? throw new ArgumentNullException(nameof(rouletteServiceClient));
    }

    public IAsyncEnumerable<RouletteRecord> QueryAsync(
        DepositAccumulateRouletteGameRecordQuery query,
        CancellationToken cancellationToken = default)
        => _rouletteServiceClient.GetRouletteRecordsAsync(
            query.BrandId,
            new RouletteRecordQuery
            {
                PlayerCodes = query.PlayerCodes,
                PromotionIds = query.PromotionIds,
                BeginTime = query.TimeRange.Begin,
                EndTime = query.TimeRange.End
            }, cancellationToken);
}
