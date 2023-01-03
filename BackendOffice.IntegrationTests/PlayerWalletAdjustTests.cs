using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.CashService.Client;
using Tgs.PlayerService.Client;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class PlayerWalletAdjustTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public PlayerWalletAdjustTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 調整玩家鎖定金額()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
            .ConfigureServices(services => services
                .AddAuthentication("Fake")
                .AddScheme<FakeAuthenticationSchemaOptions, FakeAuthenticationHandler>(
                    "Fake",
                    options => options.UserId = userId)
                .Services
                .AddSingleton(fakePlayerServiceClient)
                .AddSingleton(fakeCashServiceClient))
            .Build();

        await web.StartAsync();

        var playerCode = "test1";
        var lockAmount = 1000M;
        var remark = "test";
        var walletId = Guid.NewGuid();

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new PlayerInfo
            {
                PlayerCode = playerCode,
                BrandId = Guid.NewGuid()
            });

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                ["DefaultWalletId"] = walletId.ToString()
            }.ToImmutableDictionary());

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode, lockAmount, remark) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                await svc.adjustPlayerLockedAmount(playerCode, lockAmount, remark);
            };
        """;

        // Act
        await StaticNodeJSService.InvokeFromStringAsync(
            nodeModule,
            args: new object[] { playerCode, lockAmount, remark });

        // Assert
        _ = fakeCashServiceClient.Received(1)
            .AdjustSystemV2Async(
                Arg.Is(walletId),
                Arg.Is(0M),
                Arg.Is(lockAmount),
                Arg.Is(remark),
                Arg.Is(default(string)),
                Arg.Is(userId),
                Arg.Any<CancellationToken>());
    }
}
