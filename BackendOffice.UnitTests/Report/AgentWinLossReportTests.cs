using MockQueryable.Moq;
using NSubstitute;
using StackExchange.Redis;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Redis;
using Tgs.MessageQueue;
using TGS.Services.BackOffice.Application.Commands.ReportGenerateAggregate;
using TGS.Services.BackOffice.Application.Models.Aggregate;
using TGS.Services.BackOffice.Infrastructure.DataService;
using TGS.Services.BackOffice.Application.Queries.ReportAgentWinLossQueries;
using Tgs.Report.Client;
using Tgs.Report.Client.Models;
using TGS.Services.BackOffice.Application.Models.ReportAgentWinLoss;
using Tgs.Report.Client.Models.AgentWinLoss;

namespace TGS.Tests.BackendOffice.UnitTests.Report;

public class AgentWinLossReportTests
{
    [Test]
    public async Task Can_Get_Master_Agent_WinLoss()
    {
        var fakeBrandService = Substitute.For<IBrandService>();
        var fakeAgentService = Substitute.For<IAgentService>();
        var fakePlayerService = Substitute.For<IPlayerService>();
        var fakeReportClient = Substitute.For<IReportClient>();

        var sut = new GetAgentWinLossQuery(fakeReportClient,
            fakeBrandService,
            fakeAgentService,
            fakePlayerService);

        var brandId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var request = new GetAgentWinLossReportRequest
        {
            BrandId = brandId,
        };

        _ = fakeBrandService.GetBrandQueries().Returns(new Brand[]
        {
            new Brand { Id = brandId,PlatformCharge=0 }
        }.AsQueryable().BuildMock());

        fakeReportClient.GetMasterAgentWinLossAsync(Arg.Any<GetMasterAgentWinLossRequest>())
            .Returns(new PagedResponse<AgentWinLossModel> { Valid = true });

        var response = await sut.GetAsync(request, default);

        Assert.IsTrue(response.Valid);

        _ = fakeReportClient.Received(1)
            .GetMasterAgentWinLossAsync(Arg.Any<GetMasterAgentWinLossRequest>());
    }

    [Test]
    public async Task Can_Get_Agent_WinLoss()
    {
        var fakeBrandService = Substitute.For<IBrandService>();
        var fakeAgentService = Substitute.For<IAgentService>();
        var fakePlayerService = Substitute.For<IPlayerService>();
        var fakeReportClient = Substitute.For<IReportClient>();

        var sut = new GetAgentWinLossQuery(fakeReportClient,
            fakeBrandService,
            fakeAgentService,
            fakePlayerService);

        var brandId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var request = new GetAgentWinLossReportRequest
        {
            BrandId = brandId,
            Username = "Username",
            UserType = Core.Models.Enums.AccountType.Agent
        };

        _ = fakeBrandService.GetBrandQueries().Returns(new Brand[]
        {
            new Brand { Id = brandId,PlatformCharge=0 }
        }.AsQueryable().BuildMock());

        _ = fakeAgentService.GetAgentQueries().Returns(new Agent[]
        {
            new Agent { Id = agentId }
        }.AsQueryable().BuildMock());

        fakeReportClient.GetAgentWinLossAsync(Arg.Any<GetAgentWinLossRequest>())
            .Returns(new PagedResponse<AgentWinLossModel> { Valid = true });

        var response = await sut.GetAsync(request, default);

        Assert.IsTrue(response.Valid);

        _ = fakeReportClient.Received(1)
            .GetAgentWinLossAsync(Arg.Any<GetAgentWinLossRequest>());
    }

    [Test]
    public async Task Can_Get_Down_Line_Agent_WinLoss()
    {
        var fakeBrandService = Substitute.For<IBrandService>();
        var fakeAgentService = Substitute.For<IAgentService>();
        var fakePlayerService = Substitute.For<IPlayerService>();
        var fakeReportClient = Substitute.For<IReportClient>();

        var sut = new GetAgentWinLossQuery(fakeReportClient,
            fakeBrandService,
            fakeAgentService,
            fakePlayerService);

        var brandId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var request = new GetAgentWinLossReportRequest
        {
            BrandId = brandId,
            Username = "Username",
            UserType = Core.Models.Enums.AccountType.Agent,
            DownLine = true
        };

        _ = fakeBrandService.GetBrandQueries().Returns(new Brand[]
        {
            new Brand { Id = brandId,PlatformCharge=0 }
        }.AsQueryable().BuildMock());

        _ = fakeAgentService.GetAgentQueries().Returns(new Agent[]
        {
            new Agent { Id = agentId }
        }.AsQueryable().BuildMock());

        fakeReportClient.GetDownLineAgentWinLossAsync(Arg.Any<GetDownLineAgentWinLossRequest>())
            .Returns(new PagedResponse<AgentWinLossModel> { Valid = true });

        var response = await sut.GetAsync(request, default);

        Assert.IsTrue(response.Valid);

        _ = fakeReportClient.Received(1)
            .GetDownLineAgentWinLossAsync(Arg.Any<GetDownLineAgentWinLossRequest>());
    }

    [Test]
    public async Task Can_Get_Agent_Player_WinLoss()
    {
        var fakeBrandService = Substitute.For<IBrandService>();
        var fakeAgentService = Substitute.For<IAgentService>();
        var fakePlayerService = Substitute.For<IPlayerService>();
        var fakeReportClient = Substitute.For<IReportClient>();

        var sut = new GetAgentWinLossQuery(fakeReportClient,
            fakeBrandService,
            fakeAgentService,
            fakePlayerService);

        var brandId = Guid.NewGuid();

        var request = new GetAgentWinLossReportRequest
        {
            BrandId = brandId,
            Username = "Username",
            UserType = Core.Models.Enums.AccountType.Player
        };

        _ = fakeBrandService.GetBrandQueries().Returns(new Brand[]
        {
            new Brand { Id = brandId,PlatformCharge=0 }
        }.AsQueryable().BuildMock());

        _ = fakePlayerService.GetPlayerQueries().Returns(new Player[]
        {
            new Player { PlayerCode = "PlayerCode" }
        }.AsQueryable().BuildMock());

        fakeReportClient.GetAgentPlayerWinLossAsync(Arg.Any<GetPlayerWinLossRequest>())
            .Returns(new PagedResponse<AgentWinLossModel> { Valid = true });

        var response = await sut.GetAsync(request, default);

        Assert.IsTrue(response.Valid);

        _ = fakeReportClient.Received(1)
            .GetAgentPlayerWinLossAsync(Arg.Any<GetPlayerWinLossRequest>());
    }
}
