using System.Linq.Expressions;
using MockQueryable.Moq;
using NSubstitute;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;
using Tgs.Report.Client;
using Tgs.Report.Client.Models;
using Tgs.Report.Client.Models.AgentWinLoss;
using TGS.Services.BackOffice.Application.Models.ReportAgentWinLoss;
using TGS.Services.BackOffice.Application.Queries.ReportAgentWinLossQueries;
using TGS.Services.BackOffice.Infrastructure.DataService;
using NUnit.Framework.Legacy;
using Tgs.Report.Client.Models.Agent;
using TGS.Services.BackOffice.Application.Queries.ReportAgentQueries;
using TGS.Services.BackOffice.Application.Models.ReportAgent;

namespace TGS.Tests.BackendOffice.UnitTests.Report;

public class AgentDividendReportTests
{
    [Test]
    public async Task Can_Get_Master_Agent_WinLoss()
    {
        var fakeBrandService = Substitute.For<IBrandService>();
        var fakeReportClient = Substitute.For<IReportClient>();

        var sut = new GetAgentDividendsProductFeeQuery(fakeReportClient);

        var brandId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var request = new GetAgentDividendProductFeeReportRequest { AgentId = agentId };

        _ = fakeBrandService.GetBrandQueries().Returns(new Brand[]
        {
            new()
            {
                Id = brandId,
                PlatformCharge = 0
            }
        }.AsQueryable().BuildMock());

        _ = fakeReportClient.GetAgentDividendProductFeeAsync(Arg.Any<GetAgentDividendProductFeeRequest>())
            .Returns(new AgentDividendProductFeeResponse
            {
                CardGameAmount = 10,
                LiveDealerAmount = 10,
                LotteryAmount = 10,
                SlotAmount = 10,
                SportsAmount = 10,
                VirtualSportsAmount = 10
            });

        var response = await sut.GetAsync(request);

        Assert.That(response.Valid, Is.EqualTo(true));
        Assert.That(response.CardGameAmount, Is.EqualTo(10.0));

        _ = fakeReportClient.Received(1)
            .GetAgentDividendProductFeeAsync(Arg.Any<GetAgentDividendProductFeeRequest>());
    }
}