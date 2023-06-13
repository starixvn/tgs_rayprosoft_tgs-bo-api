using System;
using System.Collections.Generic;
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
        => _cashQueryClient.QueryAgentAdjustRecordsAsync(
            query.BrandId,
            new AdjustRecordQueryCondition(
                query.CreatedOnRange,
                query.SearchTypes,
                query.AgentId?.ToString(),
                query.Operator),
            query.Skip,
            cancellationToken);
}
