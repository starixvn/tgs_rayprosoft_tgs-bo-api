using System.Runtime.InteropServices;
using Jering.Javascript.NodeJS;
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
using TGS.Services.BackOffice.Application.Models.AgentAdjust;
using TGS.Services.BackOffice.Application.Models.ReportAgent;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class AgentTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public AgentTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
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
        var actual = await StaticNodeJSService.InvokeFromStringAsync<PagedResponse<AgentPlayerWinLossReportModel>>(
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
        var actual = await StaticNodeJSService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { request });

        // Assert
        Assert.True(actual?.Valid);

        _ = fakeAgentClient
            .Received(1)
            .SetAgentPayPasswordAsync(Arg.Is(new SetAgentPayPasswordRequest(agentId,newPayPassword)),Arg.Any<CancellationToken>());

        _ = fakeAgentClient
            .Received(1)
            .AddAgentAuditLogAsync(
                Arg.Is(new AddAgentAuditLogRequest(agentId, Tgs.Agent.Client.Models.Enums.AgentOperation.PayPassword, "system")), 
                Arg.Any<CancellationToken>());

    }
}
