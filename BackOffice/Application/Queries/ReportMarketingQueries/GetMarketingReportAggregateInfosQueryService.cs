using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.MarketingReport.Client;
using Tgs.Paging;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Services.BackOffice.Application.Models.ReportMarketing;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.ApplicationServices.Payment;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Queries.ReportMarketingQueries;

internal class GetMarketingReportAggregateInfosQueryService
    : IGetService<GetMarketingReportAggregateInfosQuery, MarketingReportResponse>
{
    private static readonly TimeSpan _Offset = TimeSpan.FromHours(8D);

    private readonly IQueryService<AgentPlayerDepositQuery, AgentPlayerDepositStatistics> _agentPlayerDepositQueryService;
    private readonly IQueryService<AllAgentsByBrandQuery, AgentInfo> _agentQueryService;
    private readonly IQueryService<PlayerPaymentStatisticsDetailQuery, PlayerDailyPaymentItem> _playerPaymentDetailQueryService;
    private readonly IQueryService<GetDailyMarketingRegisterDetailQuery, PlayerDailyRegisterItem> _playerRegisterDetailQueryService;
    private readonly IMarketingReportClient _reportClient;

    public GetMarketingReportAggregateInfosQueryService(
        IQueryService<AgentPlayerDepositQuery, AgentPlayerDepositStatistics> agentPlayerDepositQueryService,
        IQueryService<AllAgentsByBrandQuery, AgentInfo> agentQueryService,
        IQueryService<PlayerPaymentStatisticsDetailQuery, PlayerDailyPaymentItem> playerPaymentDetailQueryService,
        IQueryService<GetDailyMarketingRegisterDetailQuery, PlayerDailyRegisterItem> playerRegisterDetailQueryService,
        IMarketingReportClient reportClient)
    {
        _agentPlayerDepositQueryService = agentPlayerDepositQueryService ?? throw new ArgumentNullException(nameof(agentPlayerDepositQueryService));
        _agentQueryService = agentQueryService ?? throw new ArgumentNullException(nameof(agentQueryService));
        _playerPaymentDetailQueryService = playerPaymentDetailQueryService ?? throw new ArgumentNullException(nameof(playerPaymentDetailQueryService));
        _playerRegisterDetailQueryService = playerRegisterDetailQueryService ?? throw new ArgumentNullException(nameof(playerRegisterDetailQueryService));
        _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));
    }

    public async ValueTask<MarketingReportResponse> GetAsync(
        GetMarketingReportAggregateInfosQuery query,
        CancellationToken cancellationToken = default)
    {
        var startDate = query.StartDate;
        var endDate = query.EndDate;

        var dates = Enumerable.Range(0, endDate.DayNumber - startDate.DayNumber)
            .Select(x => startDate.AddDays(x))
            .ToArray();

        var aggregateInfos = await _reportClient
            .GetDailyAggregateInfosAsync(query.BrandId, startDate, endDate, cancellationToken)
            .ToArrayAsync(cancellationToken);

        var depositInfos = _agentPlayerDepositQueryService.QueryAsync(
            new AgentPlayerDepositQuery(query.BrandId),
            cancellationToken);

        var depositStatistics = await depositInfos
            .Where(s => s.ReportDate >= startDate && s.ReportDate < endDate)
            .ToArrayAsync(cancellationToken);

        var agents = await _agentQueryService
            .QueryAsync(new AllAgentsByBrandQuery(query.BrandId), cancellationToken)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var agentNameDict = agents
            .ToDictionary(
                a => a.AgentId,
                a => a.UserName);

        var dataQuery = from reportDate in dates
                        from p in agentNameDict
                        join deposit in depositStatistics
                            on new { ReportDate = reportDate, AgentId = p.Key } equals new { deposit.ReportDate, deposit.AgentId } into gd
                        from deposit in gd.DefaultIfEmpty()
                        join info in aggregateInfos
                            on new { ReportDate = reportDate, AgentId = p.Key } equals new { info.ReportDate, info.AgentId }
                            into ga
                        from info in ga.DefaultIfEmpty()
                        select new MarketingReportModel
                        {
                            ReportDate = reportDate,
                            AgentId = p.Key,
                            AgentUsername = p.Value,
                            RegisterCount = info?.RegisterCount ?? 0,
                            FirstDepositCount = deposit?.FirstDepositPlayers ?? 0,
                            GamingCount = info?.GamingCount ?? 0
                        };

        var currentDate = DateOnly.FromDateTime(DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(8D)).Date);

        if (endDate >= currentDate)
        {
            var currentReportDate = new DateTimeOffset(
                currentDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified),
                _Offset);

            var currentDatePaymentQuery = new PlayerPaymentStatisticsDetailQuery(
                query.BrandId,
                currentDate);
            var currentDatePaymentDetailQuery = await _playerPaymentDetailQueryService
                .QueryAsync(currentDatePaymentQuery, cancellationToken)
                .GroupBy(item => item.AgentId)
                .SelectAwait(async g => new
                {
                    AgentId = g.Key,
                    FirstDepositPlayers = await g.CountAsync(
                            item => item.IsFirstDeposit == true,
                            cancellationToken)
                            .ConfigureAwait(false)
                })
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            var currentDateRegisterDetailQuery = await _playerRegisterDetailQueryService
                .QueryAsync(new GetDailyMarketingRegisterDetailQuery(query.BrandId, currentDate), cancellationToken)
                .GroupBy(item => item.AgentId)
                .SelectAwait(async g => new
                {
                    AgentId = g.Key,
                    RegisteredPlayers = await g.CountAsync(cancellationToken)
                        .ConfigureAwait(false)
                })
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            var currentDateQuery = from p in agentNameDict
                                   join playerPayment in currentDatePaymentDetailQuery
                                    on p.Key equals playerPayment.AgentId into pg
                                   from playerPayment in pg.DefaultIfEmpty()
                                   join playerRegister in currentDateRegisterDetailQuery
                                    on p.Key equals playerRegister.AgentId into prg
                                   from playerRegister in prg.DefaultIfEmpty()
                                   select new MarketingReportModel
                                   {
                                       ReportDate = currentDate,
                                       AgentId = p.Key,
                                       AgentUsername = p.Value,
                                       RegisterCount = playerRegister?.RegisteredPlayers ?? 0,
                                       FirstDepositCount = playerPayment?.FirstDepositPlayers ?? 0,
                                       GamingCount = 0
                                   };

            dataQuery = dataQuery.Concat(currentDateQuery);
        }

        var paging = new Paging<MarketingReportModel>();
        var propertyMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["reportDate"] = nameof(MarketingReportModel.ReportDate),
            ["registerCount"] = nameof(MarketingReportModel.RegisterCount),
            ["firstDepositCount"] = nameof(MarketingReportModel.FirstDepositCount),
            ["gamingCount"] = nameof(MarketingReportModel.GamingCount)
        };

        if (!string.IsNullOrWhiteSpace(query.Paging.SortBy))
            paging.SortByPropertyName(propertyMapping[query.Paging.SortBy], query.Paging.IsDesc);

        paging.Page = query.Paging.Page;
        paging.RowCount = query.Paging.Rows;

        var totalCount = dataQuery.Count();

        var summary = dataQuery.Aggregate(
            new MarketingReportTotalModel(),
            (seed, row) =>
            {
                seed.RegisterCount += row.RegisterCount;
                seed.FirstDepositCount += row.FirstDepositCount;
                seed.GamingCount += row.GamingCount;

                return seed;
            });

        return new MarketingReportResponse
        {
            Valid = true,
            TotalCount = totalCount,
            Items = dataQuery.AsQueryable()
                .ApplyOrderBy(paging)
                .ApplyPaging(paging),
            Summary = summary
        };
    }
}
