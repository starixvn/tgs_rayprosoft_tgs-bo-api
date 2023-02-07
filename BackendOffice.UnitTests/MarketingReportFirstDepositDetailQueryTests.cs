using NSubstitute;
using Tgs.ServiceUtility;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Queries.ReportMarketingQueries;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using TGS.Services.BackOffice.ApplicationServices.Payment;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests;

internal class MarketingReportFirstDepositDetailQueryTests
{
    [Test]
    public async Task Can_Get_Marketing_Report_First_Deposit_Detail()
    {
        var fakeQueryService =
            Substitute.For<IQueryService<PlayerPaymentStatisticsDetailQuery, PlayerDailyPaymentItem>>();
        var fakeAgentQueryService = Substitute.For<IQueryService<AllAgentsByBrandQuery, AgentInfo>>();

        var paging = new PageableRequest
        {
            Page = 1,
            Rows = 10,
            ShowAll = false
        };

        var query = new GetMarketingFirstDepositDetailQuery(
            Guid.NewGuid(),
            DateOnly.Parse("2022-06-01"),
            "Agent123",
            paging);

        var sut = new GetMarketingFirstDepositDetailQueryService(
            fakeQueryService,
            fakeAgentQueryService);

        _ = await sut.GetAsync(query).ConfigureAwait(false);

        _ = fakeQueryService
            .Received(1)
            .QueryAsync(Arg.Is(new PlayerPaymentStatisticsDetailQuery(query.BrandId, query.Date)));
    }
}
