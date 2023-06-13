using System;
using System.Collections.Generic;
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
        => _cashQueryClient.QueryPlayerAdjustRecordsAsync(
            query.BrandId,
            new AdjustRecordQueryCondition(
                query.CreatedOnRange,
                query.SearchTypes,
                query.PlayerCode,
                query.Operator),
            query.Skip,
            cancellationToken);
}
