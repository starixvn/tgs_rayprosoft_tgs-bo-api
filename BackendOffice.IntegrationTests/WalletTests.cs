using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.CashService.Client;
using Tgs.PlayerService.Client;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;
public class WalletTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
{
    [Fact]
    public async Task 取得玩家主錢包餘額()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
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
                .AddSingleton(fakePlayerServiceClient)
                .AddSingleton(fakeCashServiceClient))
            .Build();

        await web.StartAsync();

        var playerCode = "test1";
        var walletId = Guid.NewGuid();

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                ["DefaultWalletId"] = walletId.ToString()
            }.ToImmutableDictionary());
        _ = fakeCashServiceClient.GetBalanceAsync(Arg.Is(walletId), Arg.Any<CancellationToken>())
            .Returns(1000M);

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-wallet-service.js').default;

            module.exports = async (playerCode) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getPlayerMainWalletBalance(playerCode);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<decimal>(
            nodeModule,
            args: new object[] { playerCode });

        // Assert
        Assert.Equal(1000M, actual);
    }
}
