using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tgs.CashQueryService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Adjust;

internal class AgentAdjustRecordsQueryService : IQueryService<AgentAdjustRecordsQuery, AdjustRecordListItem>
{
    private readonly ICashQueryClient _cashQueryClient;

    public AgentAdjustRecordsQueryService(ICashQueryClient cashQueryClient)
    {
        _cashQueryClient = cashQueryClient ?? throw new ArgumentNullException(nameof(cashQueryClient));
    }

    public IAsyncEnumerable<AdjustRecordListItem> QueryAsync(AgentAdjustRecordsQuery query, CancellationToken cancellationToken = default)
        => AsyncEnumerableEx.Return(new AdjustRecordQueryCondition(
            query.CreatedOnRange,
            query.SearchTypes,
            query.AgentId?.ToString(),
            query.Operator))
            .SelectMany(condition => _cashQueryClient.CountAgentAdjustRecordsAsync(
                query.BrandId,
                condition,
                cancellationToken)
                .AsTask()
                .ToAsyncEnumerable()
                .SelectMany(count => _cashQueryClient.QueryAgentAdjustRecordsAsync(
                    query.BrandId,
                    condition,
                    count,
                    query.Skip,
                    cancellationToken)));
}
