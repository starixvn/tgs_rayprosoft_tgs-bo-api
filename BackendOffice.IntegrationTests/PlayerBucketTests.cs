using System.Runtime.InteropServices;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.PlayerService.Client;
using Tgs.Service.PlayerBuckets;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class PlayerBucketTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public PlayerBucketTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 清除玩家流水()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();

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
                .AddSingleton(fakePlayerServiceClient)
                .AddSingleton(fakePlayerBucketClient))
            .Build();

        await web.StartAsync();

        var playerCode = "test1";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{(
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? "true"
                        : "false")}});

                let svc = new BackOfficeService(instance);

                return await svc.clearPlayerBuckets(playerCode);
            };
        """;

        _ = fakePlayerBucketClient.ReadBucketsAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new ReadBucketResult(100M, Guid.NewGuid()));

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new PlayerInfo());

        // Act
        await StaticNodeJSService.InvokeFromStringAsync(
            nodeModule,
            args: new object[] { playerCode });

        // Assert
        _ = fakePlayerBucketClient.Received(1)
            .CleanBucketsAsync(
                Arg.Is(playerCode),
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }
}
