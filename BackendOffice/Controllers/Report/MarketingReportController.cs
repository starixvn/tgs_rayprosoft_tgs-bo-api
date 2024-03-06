using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.MarketingReport.Client;
using Tgs.Paging;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Models.ReportMarketing;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using TGS.Services.BackOffice.ApplicationServices.Payment;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Report;

[Route("api/[controller]")]
[ApiController]
public class MarketingReportController : BaseContextController
{
    [HttpGet("gamingDetail")]
    [Description("Get gaming details report")]
    public async Task<PagedResponse<MarketingGamingDetailReportModel>> GetMarketingGamingDetailsAsync(
        [FromServices] IQueryService<AgentPlayerGamingDetailQuery, PlayerGamingDetail> queryService,
        [FromServices] IQueryService<AllAgentsByBrandQuery, AgentInfo> queryAgentsService,
        [FromQuery] GetMarketingGamingDetailReportRequest request)
    {
        var agentQuery = queryAgentsService
           .QueryAsync(new AllAgentsByBrandQuery(request.BrandId), HttpContext.RequestAborted);

        var agentDict = await agentQuery
            .ToDictionaryAsync(
                a => a.UserName,
                a => a.AgentId,
                HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var agentId = agentDict[request.AgentUsername];

        var query = new AgentPlayerGamingDetailQuery(request.BrandId, request.ReportDate);
        var statisticsData = queryService.QueryAsync(query, HttpContext.RequestAborted)
            .Where(item => item.AgentId == agentId);

        if (!string.IsNullOrWhiteSpace(request.PlayerUsername))
            statisticsData = statisticsData.Where(item => string.Equals(
                item.PlayerName,
                request.PlayerUsername,
                StringComparison.OrdinalIgnoreCase));

        var dataQuery = (await statisticsData
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false)).AsQueryable();

        var paging = new Paging<PlayerGamingDetail>();

        if (!string.IsNullOrWhiteSpace(request.SortBy))
            paging.SortByPropertyName(request.SortBy, request.IsDesc);

        paging.Page = request.Page;
        paging.RowCount = request.Rows;

        var totalCount = dataQuery.Count();

        dataQuery = dataQuery
            .ApplyOrderBy(paging)
            .ApplyPaging(paging);

        return new PagedResponse<MarketingGamingDetailReportModel>
        {
            TotalCount = totalCount,
            Items = [
                .. dataQuery.Select(item => new MarketingGamingDetailReportModel
                {
                    Username = item.PlayerName,
                    GameProviderName = item.GpCode,
                    SubGameProviderName = item.SubGpCode,
                    WinLossAmount = item.WinLossAmount
                })
            ]
        };
    }

    [HttpGet]
    [Description("Get marketing report")]
    public async Task<MarketingReportResponse> GetMarketingListAsync(
        [FromServices] IQueryService<AgentMarketingReportQuery, DailyAggregateInfo> queryService,
        [FromServices] IQueryService<AgentPlayerDepositQuery, AgentPlayerDepositStatistics> queryPaymentService,
        [FromServices] IQueryService<AllAgentsByBrandQuery, AgentInfo> queryAgentsService,
        [FromServices] IQueryService<PlayerPaymentStatisticsDetailQuery, PlayerDailyPaymentItem> queryPaymentDetailService,
        [FromServices] IQueryService<AgentPlayerRegisterDetailQuery, PlayerRegisterDetail> queryRegisterDetailService,
        [FromServices] IQueryService<AgentPlayerGamingDetailQuery, PlayerGamingDetail> queryGamingDetailService,
        [FromQuery] GetMarketingListReportRequest request,
        [FromQuery] PageableRequest pagingRequest)
    {
        var start = DateOnly.FromDateTime(request.From.ToOffset(TimeSpan.FromHours(8D)).DateTime);
        var end = DateOnly.FromDateTime(request.To.ToOffset(TimeSpan.FromHours(8D)).DateTime);

        var agentQuery = queryAgentsService
            .QueryAsync(new AllAgentsByBrandQuery(request.BrandId), HttpContext.RequestAborted);

        var agentDict = await agentQuery
            .ToDictionaryAsync(
                a => a.UserName,
                a => a.AgentId,
                StringComparer.OrdinalIgnoreCase,
                HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var paymentData = queryPaymentService.QueryAsync(new(request.BrandId), HttpContext.RequestAborted)
            .Where(item => item.ReportDate >= start
                && item.ReportDate < end);
        var marketingData = queryService.QueryAsync(new(request.BrandId), HttpContext.RequestAborted)
            .Where(item => item.ReportDate >= start
                && item.ReportDate < end);

        var dateArray = Enumerable.Range(0, end.DayNumber - start.DayNumber)
            .Select(n => start.AddDays(n));

        var dataQuery = from date in dateArray.ToAsyncEnumerable()
                        from agentId in agentDict.Values.ToAsyncEnumerable()
                        join marketingItem in marketingData
                            on new { ReportDate = date, AgentId = agentId } equals new { marketingItem.ReportDate, marketingItem.AgentId } into gm
                        from marketingItem in gm.DefaultIfEmpty()
                        join paymentItem in paymentData
                            on new { ReportDate = date, AgentId = agentId } equals new { paymentItem.ReportDate, paymentItem.AgentId } into gp
                        from paymentItem in gp.DefaultIfEmpty()
                        select new
                        {
                            ReportDate = date,
                            AgentId = agentId,
                            RegisterCount = marketingItem?.RegisterCount ?? 0,
                            FirstDepositPlayers = paymentItem?.FirstDepositPlayers ?? 0,
                            GamingCount = marketingItem?.GamingCount ?? 0
                        };

        var utcDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);
        var toDate = end;
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8D)).Date);
        if (toDate > today)
            toDate = today;

        if (toDate >= utcDate)
        {
            dataQuery = dataQuery.Where(item => item.ReportDate < utcDate);

            for (var i = 0; i < toDate.DayNumber - utcDate.DayNumber + 1; i++)
            {
                var queryDate = utcDate.AddDays(i);

                var currentDateRegisterQuery = new AgentPlayerRegisterDetailQuery(request.BrandId, queryDate);
                var currentDateRegisterDetails = queryRegisterDetailService.QueryAsync(currentDateRegisterQuery, HttpContext.RequestAborted)
                    .GroupBy(item => item.AgentId)
                    .SelectAwait(async g => new
                    {
                        AgentId = g.Key,
                        RegisterCount = await g
                            .CountAsync(HttpContext.RequestAborted)
                            .ConfigureAwait(false)
                    });

                var currentDateGamingQuery = new AgentPlayerGamingDetailQuery(request.BrandId, queryDate);
                var currentDateGamingDetails = queryGamingDetailService.QueryAsync(currentDateGamingQuery, HttpContext.RequestAborted)
                    .GroupBy(item => item.AgentId)
                    .SelectAwait(async g => new
                    {
                        AgentId = g.Key,
                        GamingCount = await g
                            .Select(item => item.PlayerCode)
                            .Distinct()
                            .CountAsync(HttpContext.RequestAborted)
                            .ConfigureAwait(false)
                    });

                var currentDatePaymentQuery = new PlayerPaymentStatisticsDetailQuery(
                    request.BrandId,
                    queryDate);
                var currentDatePaymentDetails = queryPaymentDetailService.QueryAsync(currentDatePaymentQuery)
                    .GroupBy(item => item.AgentId)
                    .SelectAwait(async g => new
                    {
                        AgentId = g.Key,
                        FirstDepositPlayers = await g
                            .CountAsync(item => item.IsFirstDeposit == true, HttpContext.RequestAborted)
                            .ConfigureAwait(false)
                    });

                dataQuery = dataQuery.Concat(
                    from ag in agentDict.Values.ToAsyncEnumerable()
                    join register in currentDateRegisterDetails
                        on ag equals register.AgentId into gr
                    from register in gr.DefaultIfEmpty()
                    join gaming in currentDateGamingDetails
                        on ag equals gaming.AgentId into gg
                    from gaming in gg.DefaultIfEmpty()
                    join payment in currentDatePaymentDetails
                        on ag equals payment.AgentId into gp
                    from payment in gp.DefaultIfEmpty()
                    select new
                    {
                        ReportDate = queryDate,
                        AgentId = ag,
                        RegisterCount = register?.RegisterCount ?? 0,
                        FirstDepositPlayers = payment?.FirstDepositPlayers ?? 0,
                        GamingCount = gaming?.GamingCount ?? 0
                    });
            }
        }

        dataQuery = dataQuery.Where(
            item => item.ReportDate >= start && item.ReportDate < end);

        if (!string.IsNullOrWhiteSpace(request.AgentUsername))
        {
            if (!agentDict.TryGetValue(request.AgentUsername, out var agentId))
                return new MarketingReportResponse
                {
                    TotalCount = 0,
                    Summary = new MarketingReportTotalModel(),
                    Items = []
                };

            dataQuery = dataQuery.Where(item => item.AgentId == agentId);
        }

        var paging = dataQuery.CreatePaging()
            .MapProperty(
                nameof(MarketingReportModel.ReportDate),
                item => item.ReportDate)
            .MapProperty(
                nameof(MarketingReportModel.GamingCount),
                item => item.GamingCount)
            .MapProperty(
                nameof(MarketingReportModel.RegisterCount),
                item => item.RegisterCount)
            .MapProperty(
                nameof(MarketingReportModel.FirstDepositCount),
                item => item.FirstDepositPlayers);

        if (!string.IsNullOrWhiteSpace(pagingRequest.SortBy))
            paging.SortByPropertyName(pagingRequest.SortBy, pagingRequest.IsDesc);

        paging.Page = pagingRequest.Page;
        paging.RowCount = pagingRequest.Rows;

        var totalCount = await dataQuery
            .CountAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);
        var summary = await dataQuery.AggregateAsync(
            new MarketingReportTotalModel(),
            (seed, statistics) =>
            {
                seed.RegisterCount += statistics.RegisterCount;
                seed.FirstDepositCount += statistics.FirstDepositPlayers;
                seed.GamingCount += statistics.GamingCount;

                return seed;
            },
            HttpContext.RequestAborted).ConfigureAwait(false);

        dataQuery = dataQuery
            .ApplyOrderBy(paging)
            .ApplyPaging(paging);

        var agentNameDict = await agentQuery
            .ToDictionaryAsync(
                a => a.AgentId,
                a => a.UserName,
                HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new MarketingReportResponse
        {
            TotalCount = totalCount,
            Summary = summary,
            Items = await dataQuery
                .Select(a => new MarketingReportModel
                {
                    ReportDate = a.ReportDate,
                    AgentUsername = agentNameDict[a.AgentId],
                    RegisterCount = a.RegisterCount,
                    FirstDepositCount = a.FirstDepositPlayers,
                    GamingCount = a.GamingCount
                })
                .ToArrayAsync(HttpContext.RequestAborted)
                .ConfigureAwait(false)
        };
    }

    [HttpGet("registerDetail")]
    [Description("Get register details report")]
    public async Task<PagedResponse<MarketingRegisterDetailReportModel>> GetMarketingRegisterDetailsAsync(
        [FromServices] IQueryService<AgentPlayerRegisterDetailQuery, PlayerRegisterDetail> queryService,
        [FromServices] IQueryService<AllAgentsByBrandQuery, AgentInfo> queryAgentsService,
        [FromQuery] GetMarketingRegisterDetailReportRequest request)
    {
        var agentQuery = queryAgentsService
           .QueryAsync(new AllAgentsByBrandQuery(request.BrandId), HttpContext.RequestAborted);

        var agentDict = await agentQuery
            .ToDictionaryAsync(
                a => a.UserName,
                a => a.AgentId,
                HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var agentId = agentDict[request.AgentUsername];

        var query = new AgentPlayerRegisterDetailQuery(request.BrandId, request.ReportDate);
        var statisticsData = await queryService.QueryAsync(query, HttpContext.RequestAborted)
            .Where(item => item.AgentId == agentId)
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var dataQuery = statisticsData.AsQueryable();

        var paging = new Paging<PlayerRegisterDetail>();

        if (!string.IsNullOrWhiteSpace(request.SortBy))
            paging.SortByPropertyName(request.SortBy, request.IsDesc);

        paging.Page = request.Page;
        paging.RowCount = request.Rows;

        var totalCount = statisticsData.Length;

        dataQuery = dataQuery
            .ApplyOrderBy(paging)
            .ApplyPaging(paging);

        return new PagedResponse<MarketingRegisterDetailReportModel>
        {
            TotalCount = totalCount,
            Items = [
                .. dataQuery.Select(item => new MarketingRegisterDetailReportModel
                {
                    Username = item.PlayerName,
                    LastLoginOn = item.LastLoginOn,
                    LoginCount = item.LoginCount,
                    RegisteredOn = item.RegisteredOn
                })
            ]
        };
    }
}
