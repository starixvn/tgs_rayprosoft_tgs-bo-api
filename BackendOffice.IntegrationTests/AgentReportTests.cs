using System.Data.SqlClient;
using System.Runtime.InteropServices;
using ExpectedObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Agent;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportAgent;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class AgentReportTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly NodeJsFixture _nodeJsFixture;

    public AgentReportTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
        _nodeJsFixture = nodeJsFixture ?? throw new ArgumentNullException(nameof(nodeJsFixture));
    }

    [Fact]
    public async Task 取得代理的玩家輸贏列表資料()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeReportClient = Substitute.For<IReportClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
            .ConfigureServices(services => services
                .AddAuthentication("Fake")
                .AddScheme<FakeAuthenticationSchemaOptions, FakeAuthenticationHandler>(
                    "Fake",
                    options =>
                    {
                        options.UserId = userId;
                        options.UserName = "system";
                    })
                .Services
                .AddSingleton(fakeReportClient))
            .Build();

        await web.StartAsync();

        var agentId = Guid.NewGuid();
        var queryCondition = new
        {
            AgentDividendPercentage = 10,
            From = DateTimeOffset.UtcNow.AddDays(-1D),
            To = DateTimeOffset.UtcNow
        };
        var paging = new
        {
            OrderBy = SortOrder.Ascending,
            Page = 1,
            Rows = 10,
            SortBy = "Username",
            ShowAll = false
        };

        var response = new Tgs.Report.Client.Models.PagedResponse<AgentPlayerWinLossModel>
        {
            Valid = true,
            Items = new[]
                {
                    new AgentPlayerWinLossModel
                    {
                        Username = "tester1",
                        CardGameWinLossAmount= 10,
                        LiveDealerWinLossAmount=20,
                        LotteryWinLossAmount=30,
                        SlotWinLossAmount=40,
                        SportsWinLossAmount=50,
                        VirtualSportsWinLossAmount =60
                    },
                    new AgentPlayerWinLossModel
                    {
                        Username = "tester2",
                        CardGameWinLossAmount= 70,
                        LiveDealerWinLossAmount=80,
                        LotteryWinLossAmount=90,
                        SlotWinLossAmount=100,
                        SportsWinLossAmount=110,
                        VirtualSportsWinLossAmount =120
                    }
                },
            TotalCount = 2
        };

        _ = fakeReportClient.GetAgentPlayerWinLossAsync(Arg.Any<GetAgentPlayerWinLossRequest>())
            .Returns(response);

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.listAgentPlayerWinloss(playerCode, request);
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<PagedResponse<AgentPlayerWinLossReportModel>>(
            nodeModule,
            args: new object[] { agentId, queryCondition, paging });

        // Assert
        Assert.True(actual?.Valid);
        Assert.Equal(2, actual?.TotalCount);
        response.Items
            .ToExpectedObject(ctx =>
            {
                _ = ctx.IgnoreEqualsOverride();
            })
            .ShouldMatch(actual?.Items);
    }
}
