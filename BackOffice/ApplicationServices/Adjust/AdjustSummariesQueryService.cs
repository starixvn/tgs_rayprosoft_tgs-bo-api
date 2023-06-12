using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tgs.CashQueryService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Adjust;

internal class AdjustSummariesQueryService : IQueryService<AdjustSummariesQuery, AdjustSummaryListItem>
{
    private readonly ICashQueryClient _cashQueryClient;

    public AdjustSummariesQueryService(ICashQueryClient cashQueryClient)
    {
        _cashQueryClient = cashQueryClient ?? throw new ArgumentNullException(nameof(cashQueryClient));
    }

    public IAsyncEnumerable<AdjustSummaryListItem> QueryAsync(
        AdjustSummariesQuery query,
        CancellationToken cancellationToken = default)
        => AsyncEnumerableEx
            .Return(new AdjustRecordQueryCondition(
                query.CreatedOnRange,
                null,
                null,
                null))
            .SelectMany(condition => AsyncEnumerableEx.Merge(
                _cashQueryClient.QueryPlayerAdjustSummariesAsync(condition, cancellationToken),
                _cashQueryClient.QueryAgentAdjustSummariesAsync(condition, cancellationToken)))
            .GroupBy(item => item.BrandId)
            .SelectMany(g => g.AggregateAsync(
                new AdjustSummaryListItem(
                    g.Key,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0),
                (seed, item) => seed with
                {
                    Company = seed.Company + item.Company,
                    Agent = seed.Agent + item.Agent,
                    System = seed.System + item.System,
                    Bonus = seed.Bonus + item.Bonus,
                    Transfer = seed.Transfer + item.Transfer,
                    AgentOther = seed.AgentOther + item.AgentOther
                },
                cancellationToken)
                .AsTask()
                .ToAsyncEnumerable());
}
