using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Paging;
using Tgs.ServiceUtility;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportMarketing;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using TGS.Services.BackOffice.ApplicationServices.Payment;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Queries.ReportMarketingQueries;

public class GetMarketingFirstDepositDetailQueryService(
     IQueryService<PlayerPaymentStatisticsDetailQuery, PlayerDailyPaymentItem> queryService,
     IQueryService<AllAgentsByBrandQuery, AgentInfo> agentQueryService)
        : IGetService<GetMarketingFirstDepositDetailQuery, PagedResponse<MarketingFirstDepositDetailReportModel>>
{
    private readonly IQueryService<AllAgentsByBrandQuery, AgentInfo> _agentQueryService = agentQueryService ?? throw new ArgumentNullException(nameof(agentQueryService));
    private readonly IQueryService<PlayerPaymentStatisticsDetailQuery, PlayerDailyPaymentItem> _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));

    public async ValueTask<PagedResponse<MarketingFirstDepositDetailReportModel>> GetAsync(
        GetMarketingFirstDepositDetailQuery query,
        CancellationToken cancellationToken = default)
    {
        var agentQuery = _agentQueryService
            .QueryAsync(new AllAgentsByBrandQuery(query.BrandId), cancellationToken);

        var agentDict = await agentQuery
            .ToDictionaryAsync(
                a => a.UserName,
                a => a.AgentId,
                cancellationToken)
            .ConfigureAwait(false);

        var detailQuery = new PlayerPaymentStatisticsDetailQuery(query.BrandId, query.Date);
        var statisticsData = await _queryService.QueryAsync(detailQuery, cancellationToken)
            .Where(item => item.AgentId == agentDict[query.AgentUsername]
                && item.IsFirstDeposit == true)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var dataQuery = statisticsData.AsQueryable();

        var paging = new Paging<PlayerDailyPaymentItem>();

        if (!string.IsNullOrWhiteSpace(query.Paging.SortBy))
            paging.SortByPropertyName(query.Paging.SortBy, query.Paging.IsDesc);

        paging.Page = query.Paging.Page;
        paging.RowCount = query.Paging.Rows;

        var totalCount = statisticsData.Length;

        dataQuery = dataQuery.ApplyPaging(paging);

        return new PagedResponse<MarketingFirstDepositDetailReportModel>
        {
            TotalCount = totalCount,
            Items = [
                .. dataQuery.Select(item => new MarketingFirstDepositDetailReportModel
                {
                    Username = item.PlayerName,
                    FirstDepositOn = item.FirstDepositOn,
                    FirstDepositAmount = item.FirstDepositAmount,
                    TotalDepositAmount = item.TotalDepositAmount,
                    TotalWithdrawAmount = item.TotalWithdrawAmount,
                    DepositSubtractWithdraw = item.TotalDepositAmount - item.TotalWithdrawAmount,
                    IpAddress = item.RegisterIp
                })
            ]
        };
    }
}
