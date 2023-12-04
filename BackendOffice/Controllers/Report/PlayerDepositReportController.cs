using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Paging;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Models;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportPlayerDeposit;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using TGS.Services.BackOffice.ApplicationServices.Payment;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Report;

[Route("api/[controller]")]
[ApiController]
public class PlayerDepositReportController : BaseContextController
{
    [HttpGet("playerDailyDepositDetails")]
    public async Task<PagedResponse<PlayerDepositDetailReportModel>> GetPlayerDailyDepositDetailsAsync(
        [FromServices] IQueryService<PlayerPaymentStatisticsDetailQuery, PlayerDailyPaymentItem> queryService,
        [FromServices] IQueryService<AllAgentsByBrandQuery, AgentInfo> queryAgentsService,
        [FromQuery] GetPlayerDepositDetailsReportRequest request)
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

        var query = new PlayerPaymentStatisticsDetailQuery(request.BrandId, request.ReportDate);
        var statisticsData = await queryService.QueryAsync(query, HttpContext.RequestAborted)
            .Where(item => item.AgentId == agentId
                && item.IsFirstDeposit is not null)
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var dataQuery = statisticsData.AsQueryable();

        var paging = new Paging<PlayerDailyPaymentItem>();

        if (!string.IsNullOrWhiteSpace(request.SortBy))
            paging.SortByPropertyName(request.SortBy, request.IsDesc);

        paging.Page = request.Page;
        paging.RowCount = request.Rows;

        var totalCount = statisticsData.Length;

        dataQuery = dataQuery
            .ApplyOrderBy(paging)
            .ApplyPaging(paging);

        return new PagedResponse<PlayerDepositDetailReportModel>
        {
            TotalCount = totalCount,
            Items = [
                .. dataQuery.Select(item => new PlayerDepositDetailReportModel
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

    [HttpGet]
    [Description("Get player deposits")]
    public async Task<PlayerDepositsViewModel> GetPlayerDepositsAsync(
        [FromServices] IQueryService<AgentPlayerDepositQuery, AgentPlayerDepositStatistics> queryService,
        [FromServices] IQueryService<AllAgentsByBrandQuery, AgentInfo> queryAgentsService,
        [FromServices] IQueryService<PlayerPaymentStatisticsDetailQuery, PlayerDailyPaymentItem> queryDetailService,
        [FromQuery] GetPlayerDepositsReportRequest request)
    {
        var query = new AgentPlayerDepositQuery(request.BrandId);

        var data = queryService.QueryAsync(query, HttpContext.RequestAborted);

        var (from, to) = (
            DateOnly.FromDateTime(request.From.ToOffset(TimeSpan.FromHours(8D)).DateTime),
            DateOnly.FromDateTime(request.To.ToOffset(TimeSpan.FromHours(8D)).DateTime));

        var dataQuery = data.Where(s => s.ReportDate >= from
            && s.ReportDate < to);

        var utcDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);
        var toDate = DateOnly.FromDateTime(request.To.ToOffset(TimeSpan.FromHours(8)).Date);
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8D)).Date);
        if (toDate > today)
            toDate = today;

        if (toDate >= utcDate)
        {
            dataQuery = dataQuery.Where(s => s.ReportDate < utcDate);

            for (var i = 0; i < toDate.DayNumber - utcDate.DayNumber + 1; i++)
            {
                var currentDateQuery = new PlayerPaymentStatisticsDetailQuery(
                    request.BrandId,
                    utcDate.AddDays(i));
                var currentDateDetailQuery = queryDetailService.QueryAsync(currentDateQuery)
                    .GroupBy(item => item.AgentId)
                    .SelectAwait(g => g.AggregateAsync(
                        new PlayerPaymentDetailAggregateInfo()
                        {
                            AgentId = g.Key
                        },
                        (seed, item) =>
                        {
                            if (item.IsFirstDeposit == true)
                                seed.FirstDepositPlayers++;
                            if (item.UnSecondDeposit == true)
                                seed.UndepositAgainPlayers++;
                            if (item.IsFirstDeposit is not null)
                                seed.DepositPlayers++;

                            return seed;
                        }, HttpContext.RequestAborted))
                    .Select(info => new AgentPlayerDepositStatistics(
                        currentDateQuery.QueryDate,
                        info.AgentId,
                        info.FirstDepositPlayers,
                        info.DepositPlayers,
                        info.UndepositAgainPlayers));

                dataQuery = dataQuery.Concat(currentDateDetailQuery);
            }
        }

        var agentQuery = queryAgentsService
            .QueryAsync(new AllAgentsByBrandQuery(request.BrandId), HttpContext.RequestAborted);

        var agentDict = await agentQuery
            .ToDictionaryAsync(
                a => a.UserName,
                a => a.AgentId,
                HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var dateArray = Enumerable.Range(0, to.DayNumber - from.DayNumber)
            .Select(n => from.AddDays(n));

        if (!string.IsNullOrWhiteSpace(request.AgentUsername))
        {
            if (!agentDict.TryGetValue(request.AgentUsername, out var agentId))
                return new PlayerDepositsViewModel
                {
                    TotalCount = 0,
                    Summary = new PlayerDepositsSummaryViewModel(),
                    Items = Array.Empty<PlayerDepositReportModel>()
                };

            dataQuery = dataQuery.Where(a => a.AgentId == agentId);

            dataQuery = from date in dateArray.ToAsyncEnumerable()
                        join statistics in dataQuery on date equals statistics.ReportDate into g
                        from statistics in g.DefaultIfEmpty()
                        select statistics ?? new AgentPlayerDepositStatistics(date, agentId, 0, 0, 0);
        }
        else
            dataQuery = from date in dateArray.ToAsyncEnumerable()
                        from p in agentDict.ToAsyncEnumerable()
                        join statistics in dataQuery
                            on new { ReportDate = date, AgentId = p.Value } equals new { statistics.ReportDate, statistics.AgentId }
                            into g
                        from statistics in g.DefaultIfEmpty()
                        select statistics ?? new AgentPlayerDepositStatistics(date, p.Value, 0, 0, 0);

        var paging = new Paging<AgentPlayerDepositStatistics>();

        if (!string.IsNullOrWhiteSpace(request.SortBy))
            paging.SortByPropertyName(request.SortBy, request.IsDesc);

        paging.Page = request.Page;
        paging.RowCount = request.Rows;

        var totalCount = await dataQuery
            .CountAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);
        var summary = await dataQuery.AggregateAsync(
            new PlayerDepositsSummaryViewModel(),
            (seed, statistics) =>
            {
                seed.DepositTotalCount += statistics.DepositPlayers;
                seed.FirstDepositTotalCount += statistics.FirstDepositPlayers;
                seed.NoSecondDepositTotalCount += statistics.NoSecondDepositPlayers;

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

        return new PlayerDepositsViewModel
        {
            TotalCount = totalCount,
            Summary = summary,
            Items = await dataQuery
                .Select(a => new PlayerDepositReportModel
                {
                    ReportDate = a.ReportDate,
                    AgentUserName = agentNameDict[a.AgentId],
                    DepositCount = a.DepositPlayers,
                    FirstDepositCount = a.FirstDepositPlayers,
                    NoSecondDepositCount = a.NoSecondDepositPlayers
                })
                .ToArrayAsync(HttpContext.RequestAborted)
                .ConfigureAwait(false)
        };
    }

    [HttpGet("playerFirstDepositDetails")]
    [HttpGet("~/api/MarketingReport/firstDepositDetail")]
    [Description("Get player first deposit details report")]
    public async Task<PagedResponse<PlayerDepositDetailReportModel>> GetPlayerFirstDepositDetailsAsync(
        [FromServices] IQueryService<PlayerPaymentStatisticsDetailQuery, PlayerDailyPaymentItem> queryService,
        [FromServices] IQueryService<AllAgentsByBrandQuery, AgentInfo> queryAgentsService,
        [FromQuery] GetPlayerDepositDetailsReportRequest request)
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

        var query = new PlayerPaymentStatisticsDetailQuery(request.BrandId, request.ReportDate);
        var statisticsData = await queryService.QueryAsync(query, HttpContext.RequestAborted)
            .Where(item => item.AgentId == agentId
                && item.IsFirstDeposit == true)
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var dataQuery = statisticsData.AsQueryable();

        var paging = new Paging<PlayerDailyPaymentItem>();

        if (!string.IsNullOrWhiteSpace(request.SortBy))
            paging.SortByPropertyName(request.SortBy, request.IsDesc);

        paging.Page = request.Page;
        paging.RowCount = request.Rows;

        var totalCount = statisticsData.Length;

        dataQuery = dataQuery
            .ApplyOrderBy(paging)
            .ApplyPaging(paging);

        return new PagedResponse<PlayerDepositDetailReportModel>
        {
            TotalCount = totalCount,
            Items = [
                .. dataQuery.Select(item => new PlayerDepositDetailReportModel
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

    [HttpGet("playerNoSecondDepositDetails")]
    public async Task<PagedResponse<PlayerDepositDetailReportModel>> GetPlayerNoSecondDepositDetailsAsync(
        [FromServices] IQueryService<PlayerPaymentStatisticsDetailQuery, PlayerDailyPaymentItem> queryService,
        [FromServices] IQueryService<AllAgentsByBrandQuery, AgentInfo> queryAgentsService,
        [FromQuery] GetPlayerNoSecondDepositDetailsReportRequest request)
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

        var query = new PlayerPaymentStatisticsDetailQuery(request.BrandId, request.ReportDate);
        var statisticsData = await queryService.QueryAsync(query, HttpContext.RequestAborted)
            .Where(item => item.AgentId == agentId
                && item.UnSecondDeposit == true)
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var dataQuery = statisticsData.AsQueryable();

        var paging = new Paging<PlayerDailyPaymentItem>();

        if (!string.IsNullOrWhiteSpace(request.SortBy))
            paging.SortByPropertyName(request.SortBy, request.IsDesc);

        paging.Page = request.Page;
        paging.RowCount = request.Rows;

        var totalCount = statisticsData.Length;

        dataQuery = dataQuery
            .ApplyOrderBy(paging)
            .ApplyPaging(paging);

        return new PagedResponse<PlayerDepositDetailReportModel>
        {
            TotalCount = totalCount,
            Items = [
                .. dataQuery.Select(item => new PlayerDepositDetailReportModel
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
