using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tgs.CashQueryService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Adjust;

internal class AdjustSummariesQueryService(ICashQueryClient cashQueryClient) : IQueryService<AdjustSummariesQuery, AdjustSummaryListItem>
{
    private readonly ICashQueryClient _cashQueryClient = cashQueryClient ?? throw new ArgumentNullException(nameof(cashQueryClient));

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
                    0,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M),
                (seed, item) => seed with
                {
                    Company = seed.Company + item.Company,
                    Agent = seed.Agent + item.Agent,
                    System = seed.System + item.System,
                    Bonus = seed.Bonus + item.Bonus,
                    Transfer = seed.Transfer + item.Transfer,
                    AgentOther = seed.AgentOther + item.AgentOther,
                    CompanyAmount = seed.CompanyAmount + item.CompanyAmount,
                    AgentAmount = seed.AgentAmount + item.AgentAmount,
                    SystemAmount = seed.SystemAmount + item.SystemAmount,
                    TransferAmount = seed.TransferAmount + item.TransferAmount,
                    BonusAmount = seed.BonusAmount + item.BonusAmount,
                    AgentOtherAmount = seed.AgentOtherAmount + item.AgentOtherAmount
                },
                cancellationToken)
                .AsTask()
                .ToAsyncEnumerable());
}
