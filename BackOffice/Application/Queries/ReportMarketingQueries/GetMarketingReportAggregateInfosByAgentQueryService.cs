using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.MarketingReport.Client;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Services.BackOffice.Application.Models.ReportMarketing;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Queries.ReportMarketingQueries;

internal class GetMarketingReportAggregateInfosByAgentQueryService
    : IGetService<GetMarketingReportAggregateInfosByAgentQuery, MarketingReportResponse>
{
    private readonly IAgentRepository _agentRepository;
    private readonly IMarketingReportClient _reportClient;

    public GetMarketingReportAggregateInfosByAgentQueryService(
        IAgentRepository agentRepository,
        IMarketingReportClient reportClient)
    {
        _agentRepository = agentRepository ?? throw new ArgumentNullException(nameof(agentRepository));
        _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));
    }

    public async ValueTask<MarketingReportResponse> GetAsync(
        GetMarketingReportAggregateInfosByAgentQuery query,
        CancellationToken cancellationToken = default)
    {
        var startDate = query.StartDate;
        var endDate = query.EndDate;

        var agent = await _agentRepository
            .GetAgentAsync(query.BrandId, query.AgentUserName, cancellationToken)
            .ConfigureAwait(false);

        if (agent == null)
        {
            return new MarketingReportResponse
            {
                Valid = true,
                Items = Array.Empty<MarketingReportModel>()
            };
        }

        var dates = Enumerable.Range(0, endDate.DayNumber - startDate.DayNumber)
            .Select(x => startDate.AddDays(x))
            .ToArray();

        var aggregateInfos = await _reportClient
            .GetDailyAggregateInfosAsync(query.BrandId, agent.AgentId, startDate, endDate, cancellationToken)
            .ToArrayAsync(cancellationToken);

        var aggregates =
            (from date in dates
             join info in aggregateInfos on date equals info.ReportDate into gj
             from items in gj.DefaultIfEmpty()
             select new MarketingReportModel
             {
                 ReportDate = date,
                 AgentId = agent.AgentId,
                 AgentUsername = agent.UserName,
                 RegisterCount = items?.RegisterCount ?? 0,
                 GamingCount = items?.GamingCount ?? 0
             }).ToArray();

        var pagingResponse = aggregates.AsQueryable().CreatePagedResponse(query.Paging);

        var total = new MarketingReportTotalModel
        {
            RegisterCount = aggregates.Sum(a => a.RegisterCount),
            FirstDepositCount = aggregates.Sum(a => a.FirstDepositCount),
            GamingCount = aggregates.Sum(a => a.GamingCount)
        };

        return new MarketingReportResponse
        {
            Valid = true,
            TotalCount = pagingResponse.TotalCount,
            Items = pagingResponse.Items,
            Summary = total
        };
    }
}
