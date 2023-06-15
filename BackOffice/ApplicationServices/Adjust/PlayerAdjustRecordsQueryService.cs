using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tgs.CashQueryService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Adjust;

internal class PlayerAdjustRecordsQueryService : IQueryService<PlayerAdjustRecordsQuery, AdjustRecordListItem>
{
    private readonly ICashQueryClient _cashQueryClient;

    public PlayerAdjustRecordsQueryService(ICashQueryClient cashQueryClient)
    {
        _cashQueryClient = cashQueryClient ?? throw new ArgumentNullException(nameof(cashQueryClient));
    }

    public IAsyncEnumerable<AdjustRecordListItem> QueryAsync(
        PlayerAdjustRecordsQuery query,
        CancellationToken cancellationToken = default)
        => AsyncEnumerableEx.Return(new AdjustRecordQueryCondition(
            query.CreatedOnRange,
            query.SearchTypes,
            query.PlayerCode,
            query.Operator))
            .SelectMany(condition => _cashQueryClient.CountPlayerAdjustRecordsAsync(
                query.BrandId,
                condition,
                cancellationToken)
                .AsTask()
                .ToAsyncEnumerable()
                .SelectMany(count => _cashQueryClient.QueryPlayerAdjustRecordsAsync(
                    query.BrandId,
                    condition,
                    count,
                    query.Skip,
                    cancellationToken)));
}
