using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using EventAction = Tgs.Agent.Client.Models.Enums.EventAction;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Agent;
using TGS.Services.BackOffice.Application.Models.AgentAdjust;
using TGS.Services.BackOffice.Application.Models.ReportAgent;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using Xunit.Abstractions;
using static Microsoft.IO.RecyclableMemoryStreamManager;
using TGS.Core.DataServices.Base.Interfaces;
using Tgs.PlayerService.Client;

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

    [Fact]
    public async Task 更新代理聯絡資訊()
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

            module.exports = async (agentId,request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.setAgentContact(agentId,request);
            };
        """;

        var agentId = Guid.Parse("5056ebbf-a975-42f1-9e97-b3299f0cab06");

        _ = fakeAgentClient.GetAgentInfoAsync(new GetAgentInfoRequest(agentId), Arg.Any<CancellationToken>())
            .Returns(new AgentInfoModel(
                agentId,
                "agentUsername",
                null,
                null,
                null,
                null,
                null,
                "jhmdpi",
                null,
                DateTimeOffset.UtcNow.AddMonths(-1),
                Tgs.Agent.Client.Models.Enums.AgentStatus.Enabled,
                Tgs.Agent.Client.Models.Enums.AgentType.Cash,
                Tgs.Agent.Client.Models.Enums.AgentLevelType.Chief,
                0m,
                0,
                0m));

        var request = new TGS.Apis.BackendOffice.ViewModels.SetAgentContactRequest
        {
            { "Phone","newPhone" }
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { agentId, request });

        // Assert
        Assert.True(actual?.Valid);
    }

    [Fact]
    public async Task 更新代理密碼()
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

            module.exports = async (agentId,password) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.setAgentPassword(agentId,password);
            };
        """;

        var agentId = Guid.Parse("5056ebbf-a975-42f1-9e97-b3299f0cab06");
        var password = "newPassword";

        _ = fakeAgentClient.GetAgentInfoAsync(new GetAgentInfoRequest(agentId), Arg.Any<CancellationToken>())
            .Returns(new AgentInfoModel(
                agentId,
                "agentUsername",
                null,
                null,
                null,
                null,
                null,
                "jhmdpi",
                null,
                DateTimeOffset.UtcNow.AddMonths(-1),
                Tgs.Agent.Client.Models.Enums.AgentStatus.Enabled,
                Tgs.Agent.Client.Models.Enums.AgentType.Cash,
                Tgs.Agent.Client.Models.Enums.AgentLevelType.Chief,
                0m,
                0,
                0m));

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { agentId, password });

        // Assert
        Assert.True(actual?.Valid);
    }

    [Fact]
    public async Task 查詢代理歷史紀錄()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeUserService = Substitute.For<ITgsQueries<TGS.Core.DataServices.Tgs.Models.User>>();

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
                .AddSingleton(fakeAgentClient)
                .AddSingleton(fakeUserService))
            .Build();

        await web.StartAsync();

        var agentId = Guid.Parse("5056ebbf-a975-42f1-9e97-b3299f0cab06");
        var from = DateTime.UtcNow.AddDays(-1);
        var to = DateTime.UtcNow;

        var conditions = new GetAgentHistoriesQueryConditions(agentId, from, to);

        _ = fakeAgentClient.CountAgentHistoriesAsync(conditions, Arg.Any<CancellationToken>())
            .Returns(3);

        var eventId = Guid.NewGuid();
        _ = fakeAgentClient.GetAgentHistoriesAsync(
            conditions, 3, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new AgentHistoryInfo[]
            {
                new AgentHistoryInfo
                {
                    Action = EventAction.Create,
                    CreatedOn = DateTimeOffset.Now.AddDays(-1),
                    EventId = eventId,
                }
            }.ToAsyncEnumerable());

        _ = fakeUserService
            .GetQueries()
            .Returns(Array.Empty<TGS.Core.DataServices.Tgs.Models.User>().AsQueryable());

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (agentId,request, page, rows) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getAgentHistories(agentId,request, page, rows);
            };
        """;

        var request = new AgentHistoriesQueryViewModel
        {
            From = from,
            To = to,
        };
        var page = 1;
        var rows = 1;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { agentId, request, page, rows });

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<PagedResponse<AgentHistoryViewModel>>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.NotNull(result);
        Assert.True(result.Valid);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(eventId, result.Items.First().EventId);
    }

    [Fact]
    public async Task 查詢代理歷史紀錄內容()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeUserService = Substitute.For<ITgsQueries<TGS.Core.DataServices.Tgs.Models.User>>();

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
                .AddSingleton(fakeAgentClient)
                .AddSingleton(fakeUserService))
            .Build();

        var eventId = Guid.NewGuid();
        _ = fakeAgentClient.GetAgentHistoryAsync(
            eventId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(
                new AgentEventInfo
                {
                    Action = EventAction.Create,
                    ChangeSets = JsonDocument.Parse("{}"),
                    CreatedOn = DateTime.UtcNow.AddDays(-1),
                    Modifier = null,
                }
            );

        _ = fakeUserService
            .GetQueries()
            .Returns(Array.Empty<TGS.Core.DataServices.Tgs.Models.User>().AsQueryable());

        await web.StartAsync();        

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (eventId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getAgentHistory(eventId);
            };
        """;
        

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { eventId });

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<AgentHistoryContentViewModel>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.NotNull(result);
        Assert.Equal(EventAction.Create, result.Action);
    }
}
