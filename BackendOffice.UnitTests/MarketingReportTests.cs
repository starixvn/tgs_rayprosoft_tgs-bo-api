using Tgs.MarketingReport.Client;
using Tgs.ServiceUtility;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Queries.ReportMarketingQueries;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using TGS.Services.BackOffice.ApplicationServices.Payment;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests;

internal class MarketingReportTests
{
    [Test]
    public async Task Can_Get_Marketing_Report_By_Agent()
    {
        var fakeAgentRepository = Substitute.For<IAgentRepository>();
        var fakeReportClient = Substitute.For<IMarketingReportClient>();

        var paging = new PageableRequest
        {
            Page = 1,
            Rows = 10,
            ShowAll = false
        };

        var agentInfo = new AgentInfo(
            Guid.NewGuid(),
            "Agent1",
            Guid.NewGuid(),
            AgentStatus.Enabled);

        var query = new GetMarketingReportAggregateInfosByAgentQuery(
            Guid.NewGuid(),
            agentInfo.UserName,
            DateOnly.Parse("2022-06-01"),
            DateOnly.Parse("2022-06-10"),
            paging);

        _ = fakeAgentRepository.GetAgentAsync(
                 Arg.Is(query.BrandId),
                 Arg.Is(query.AgentUserName))
             .Returns(agentInfo);

        var sut = new GetMarketingReportAggregateInfosByAgentQueryService(
            fakeAgentRepository,
            fakeReportClient);

        _ = await sut.GetAsync(query).ConfigureAwait(false);

        _ = fakeAgentRepository
            .Received(1)
            .GetAgentAsync(
                Arg.Is(query.BrandId),
                Arg.Is(query.AgentUserName));

        _ = fakeReportClient
            .Received(1)
            .GetDailyAggregateInfosAsync(
                Arg.Is(query.BrandId),
                Arg.Is(agentInfo.AgentId),
                Arg.Is(query.StartDate),
                Arg.Is(query.EndDate));
    }

    [Test]
    public async Task Can_Get_Marketing_Report_By_Brand()
    {
        var fakeReportClient = Substitute.For<IMarketingReportClient>();
        var fakeAgentPlayerDepositQueryService = Substitute.For<IQueryService<AgentPlayerDepositQuery, AgentPlayerDepositStatistics>>();
        var fakeAgentQueryService = Substitute.For<IQueryService<AllAgentsByBrandQuery, AgentInfo>>();
        var fakePlayerPaymentDetailQueryService = Substitute.For<IQueryService<PlayerPaymentStatisticsDetailQuery, PlayerDailyPaymentItem>>();
        var fakeDailMarketingRegisterQueryService = Substitute.For<IQueryService<GetDailyMarketingRegisterDetailQuery, PlayerDailyRegisterItem>>();

        var sut = new GetMarketingReportAggregateInfosQueryService(
            fakeAgentPlayerDepositQueryService,
            fakeAgentQueryService,
            fakePlayerPaymentDetailQueryService,
            fakeDailMarketingRegisterQueryService,
            fakeReportClient);

        var paging = new PageableRequest
        {
            Page = 1,
            Rows = 10,
            ShowAll = false
        };

        var query = new GetMarketingReportAggregateInfosQuery(
            Guid.NewGuid(),
            DateOnly.Parse("2022-06-01"),
            DateOnly.Parse("2022-06-10"),
            paging);

        _ = fakeAgentQueryService
            .QueryAsync(Arg.Is(new AllAgentsByBrandQuery(query.BrandId)))
            .Returns(new[] {
                new AgentInfo(
                    Guid.NewGuid(),
                    "Agent1",
                    Guid.NewGuid(),
                    AgentStatus.Enabled)
            }.ToAsyncEnumerable());

        _ = await sut.GetAsync(query).ConfigureAwait(false);

        _ = fakeAgentPlayerDepositQueryService
            .Received(1)
            .QueryAsync(Arg.Is(new AgentPlayerDepositQuery(query.BrandId)));

        _ = fakeReportClient
            .Received(1)
            .GetDailyAggregateInfosAsync(
                Arg.Is(query.BrandId),
                Arg.Is(query.StartDate),
                Arg.Is(query.EndDate));
    }

    [Test]
    public async Task Get_MarketingReport_By_Brand_Id()
    {
        var fakeMarketingReportClient = Substitute.For<IMarketingReportClient>();

        var sut = new AgentMarketingReportQueryService(fakeMarketingReportClient);

        var query = new AgentMarketingReportQuery(Guid.NewGuid());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakeMarketingReportClient.Received(1)
            .QueryMarketingStatisticsAsync(Arg.Is(query.BrandId), Arg.Any<DateOnly>());
    }

    [Test]
    public async Task Get_MarketingReport_By_Brand_Sort_PropertyName()
    {
        var fakeReportClient = Substitute.For<IMarketingReportClient>();
        var fakeAgentPlayerDepositQueryService = Substitute.For<IQueryService<AgentPlayerDepositQuery, AgentPlayerDepositStatistics>>();
        var fakeAgentQueryService = Substitute.For<IQueryService<AllAgentsByBrandQuery, AgentInfo>>();
        var fakePlayerPaymentDetailQueryService = Substitute.For<IQueryService<PlayerPaymentStatisticsDetailQuery, PlayerDailyPaymentItem>>();
        var fakeDailMarketingRegisterQueryService = Substitute.For<IQueryService<GetDailyMarketingRegisterDetailQuery, PlayerDailyRegisterItem>>();

        var sut = new GetMarketingReportAggregateInfosQueryService(
            fakeAgentPlayerDepositQueryService,
            fakeAgentQueryService,
            fakePlayerPaymentDetailQueryService,
            fakeDailMarketingRegisterQueryService,
            fakeReportClient);

        var paging = new PageableRequest
        {
            Page = 1,
            Rows = 10,
            SortBy = "reportDate",
            ShowAll = false
        };

        var query = new GetMarketingReportAggregateInfosQuery(
            Guid.NewGuid(),
            DateOnly.Parse("2022-06-01"),
            DateOnly.Parse("2022-06-10"),
            paging);

        _ = fakeAgentQueryService
            .QueryAsync(Arg.Is(new AllAgentsByBrandQuery(query.BrandId)))
            .Returns(new[] {
                new AgentInfo(
                    Guid.NewGuid(),
                    "Agent1",
                    Guid.NewGuid(),
                    AgentStatus.Enabled)
            }.ToAsyncEnumerable());

        _ = await sut.GetAsync(query).ConfigureAwait(false);

        _ = fakeAgentPlayerDepositQueryService
            .Received(1)
            .QueryAsync(Arg.Is(new AgentPlayerDepositQuery(query.BrandId)));

        _ = fakeReportClient
            .Received(1)
            .GetDailyAggregateInfosAsync(
                Arg.Is(query.BrandId),
                Arg.Is(query.StartDate),
                Arg.Is(query.EndDate));
    }

    [Test]
    public async Task Get_Player_Gaming_Detail()
    {
        var fakeMarketingReportClient = Substitute.For<IMarketingReportClient>();

        var sut = new AgentPlayerGamingDetailQueryService(fakeMarketingReportClient);

        var query = new AgentPlayerGamingDetailQuery(Guid.NewGuid(), DateOnly.Parse("2022-8-15"));

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakeMarketingReportClient.Received(1)
            .QueryPlayerGamingDetailAsync(Arg.Is(query.BrandId), Arg.Is(query.ReportDate));
    }

    [Test]
    public async Task Get_Player_Register_Detail()
    {
        var fakeMarketingReportClient = Substitute.For<IMarketingReportClient>();

        var sut = new AgentPlayerRegisterDetailQueryService(fakeMarketingReportClient);

        var query = new AgentPlayerRegisterDetailQuery(Guid.NewGuid(), DateOnly.Parse("2022-8-15"));

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakeMarketingReportClient.Received(1)
            .QueryPlayerRegisterDetailAsync(Arg.Is(query.BrandId), Arg.Is(query.ReportDate));
    }

    [Test]
    public void Get_RealTime_Gaming_Detail()
    {
        _ = new GetDailyMarketingGamingDetailQueryService();
    }

    [Test]
    public async Task Get_RealTime_PlayerRegister_Statistics()
    {
        var fakeMarketingReportRepository = Substitute.For<IMarketingReportRepository>();

        var sut = new GetDailyMarketingRegisterDetailQueryService(fakeMarketingReportRepository);

        var query = new GetDailyMarketingRegisterDetailQuery(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Now));

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakeMarketingReportRepository.Received(1)
            .QueryRealTimePlayerDailyRegisterStatisticsAsync(Arg.Is(query.BrandId), Arg.Is(query.QueryDate));
    }
}
