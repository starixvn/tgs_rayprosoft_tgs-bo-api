using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Agent;
using TGS.Services.BackOffice.Application.Models.AgentAdjust;
using TGS.Services.BackOffice.Application.Models.ReportAgent;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class AgentTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly NodeJsFixture _nodeJsFixture;

    public AgentTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
        _nodeJsFixture = nodeJsFixture ?? throw new ArgumentNullException(nameof(nodeJsFixture));
    }

    [Fact]
    public async Task 代理扣錢()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeService = Substitute.For<IGetService<AgentReduceAmountQuery, AgentReduceAmountResult>>();

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
                .AddSingleton(fakeService))
            .Build();

        await web.StartAsync();

        _ = fakeService.GetAsync(Arg.Any<AgentReduceAmountQuery>())
            .Returns(new AgentReduceAmountResult(true, string.Empty));

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (id, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.createAgentReduceAmount(id, request);
            };
        """;

        var agentId = Guid.Parse("5056ebbf-a975-42f1-9e97-b3299f0cab06");
        var request = new CreateAgentReduceAmountRequest
        {
            AccountType = AccountType.Player,
            AgentId = agentId,
            Amount = 1,
            Remark = "20230822",
            TargetName = "shaun01",
            TransactionType = AgentReduceTransactionType.Transfer
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<PagedResponse<AgentPlayerWinLossReportModel>>(
            nodeModule,
            args: new object[] { agentId, request });

        // Assert
        Assert.True(actual?.Valid);
    }

    [Fact]
    public async Task 更新代理提款密碼()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeAgentClient = Substitute.For<IAgentClient>();

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
                .AddSingleton(fakeAgentClient))
            .Build();

        await web.StartAsync();

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.updateAgentPayPassword(request);
            };
        """;

        var agentId = Guid.Parse("5056ebbf-a975-42f1-9e97-b3299f0cab06");
        var newPayPassword = "newPassword";
        var request = new UpdateAgentPayPasswordRequest
        {
            AgentId = agentId,
            PayPassword = newPayPassword
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { request });

        // Assert
        Assert.True(actual?.Valid);

        _ = fakeAgentClient
            .Received(1)
            .SetAgentPayPasswordAsync(Arg.Is(new SetAgentPayPasswordRequest(agentId, newPayPassword)), Arg.Any<CancellationToken>());

        _ = fakeAgentClient
            .Received(1)
            .AddAgentAuditLogAsync(
                Arg.Is(new AddAgentAuditLogRequest(agentId, Tgs.Agent.Client.Models.Enums.AgentOperation.PayPassword, "system")),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 查詢代理操作紀錄()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeAgentClient = Substitute.For<IAgentClient>();

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
                .AddSingleton(fakeAgentClient))
            .Build();

        await web.StartAsync();

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (id,paging,request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getAgentOperations(id,paging,request);
            };
        """;

        var id = Guid.Parse("5056ebbf-a975-42f1-9e97-b3299f0cab06");
        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var from = DateTimeOffset.Now.AddDays(-14);
        var to = DateTimeOffset.Now;

        _ = fakeAgentClient
           .GetAgentAuditLogsAsync(
                Arg.Any<GetAgentAuditLogQueryConditions>(),
                Arg.Any<Paging>(),
                Arg.Any<CancellationToken>())
           .Returns(new PagingResponse<AgentAuditLogInfo>
           {
               Items = new AgentAuditLogInfo[]
               {
                   new AgentAuditLogInfo
                   {
                       CreatedByUsername = "username",
                       CreatedOn = DateTimeOffset.Now.AddDays(-1),
                       Id = Guid.NewGuid(),
                       Operation = Tgs.Agent.Client.Models.Enums.AgentOperation.PayPassword
                   }
               },
               TotalCount = 1,
               Rows = 20,
               Page = 1
           });

        var paging = new PageableRequest
        {
            Rows = 20,
            Page = 1,
            ShowAll = false,
            SortBy = "createdOn",
            OrderBy = System.Data.SqlClient.SortOrder.Descending,
        };
        var request = new GetAgentOperationsRequest
        {
            BrandId = brandId,
            From = from,
            To = to,
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { id, paging, request });

        // Assert
        Assert.True(actual?.Valid);
    }
}
