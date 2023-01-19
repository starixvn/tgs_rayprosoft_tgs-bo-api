using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Tgs.CashService.Client;
using TGS.Core.Models.Enums;
using Tgs.PlayerService.Client;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Domain;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace BackendOffice.IntegrationTests;
public class AgentDomainTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public AgentDomainTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
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
        await StaticNodeJSService.InvokeFromStringAsync(
            nodeModule,
            args: new object[] { brandId, agentId, domain });

        // Assert
        _ = fakeAgentRepository.Received(1)
            .AddDomainToAgentAsync(
                Arg.Is(agentId),
                Arg.Is(domain));
    }
}
