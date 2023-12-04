using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.Domain;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class AgentDomainTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    private readonly NodeJsFixture _nodeJsFixture = nodeJsFixture ?? throw new ArgumentNullException(nameof(nodeJsFixture));

    [Fact]
    public async Task 取得代理域名列表()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeAgentRepository = Substitute.For<IAgentRepository>();

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
                .AddSingleton(fakeAgentRepository))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var expected = new ResponseViewModel<string[]>
        {
            Valid = true,
            Data = [
                "test.com",
                "test1.com"]
        };

        _ = fakeAgentRepository.QueryDomainsFromAgentAsync(Arg.Is(agentId))
            .Returns(expected.Data.ToAsyncEnumerable());

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, agentId, domain) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.listAgentDomains(brandId, agentId, domain);
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<ResponseViewModel<string[]>>(
            nodeModule,
            args: new object[] { brandId, agentId });

        // Assert
        Assert.True(actual?.Valid);
        Assert.Equal(expected.Data, actual?.Data);
    }

    [Fact]
    public async Task 移除代理域名()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeAgentRepository = Substitute.For<IAgentRepository>();

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
                .AddSingleton(fakeAgentRepository))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var domain = "test.com";

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, agentId, domain) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                await svc.removeAgentDomain(brandId, agentId, domain);
            };
        """;

        // Act
        await nodeService.InvokeFromStringAsync(
            nodeModule,
            args: new object[] { brandId, agentId, domain });

        // Assert
        _ = fakeAgentRepository.Received(1)
            .RemoveDomainFromAgentAsync(
                Arg.Is(agentId),
                Arg.Is(domain));
    }

    [Fact]
    public async Task 新增代理域名資料()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeAgentRepository = Substitute.For<IAgentRepository>();

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
                .AddSingleton(fakeAgentRepository))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var domain = "test.com";

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, agentId, domain) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                await svc.addAgentDomain(brandId, agentId, domain);
            };
        """;

        // Act
        await nodeService.InvokeFromStringAsync(
            nodeModule,
            args: new object[] { brandId, agentId, domain });

        // Assert
        _ = fakeAgentRepository.Received(1)
            .AddDomainToAgentAsync(
                Arg.Is(agentId),
                Arg.Is(domain));
    }
}
