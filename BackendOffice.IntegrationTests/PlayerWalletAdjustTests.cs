using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.CashService.Client;
using Tgs.PlayerService.Client;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Domain;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class PlayerWalletAdjustTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly NodeJsFixture _nodeJsFixture;

    public PlayerWalletAdjustTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
        _nodeJsFixture = nodeJsFixture ?? throw new ArgumentNullException(nameof(nodeJsFixture));
    }

    [Fact]
    public async Task 玩家調帳加款()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerAdjustBalanceService = Substitute.For<IPlayerAdjustBalanceService>();

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
                .AddSingleton(fakeCashServiceClient)
                .AddSingleton(fakePlayerAdjustBalanceService))
            .Build();

        await web.StartAsync();

        var playerCode = "test1";
        var request = new CreatePlayerReduceAmountRequest
        {
            Amount = 1000M,
            BucketAmount = 0M,
            IncludeInNetProfit = true,
            Remark = "test",
            TransactionType = PlayerReduceTransactionType.SystemAdjust
        };
        var walletId = Guid.NewGuid();
        var brandId = Guid.NewGuid();

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Tgs.PlayerService.Client.PlayerInfo
            {
                PlayerCode = playerCode,
                UserName = "tester",
                BrandId = brandId
            });

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                ["DefaultWalletId"] = walletId.ToString()
            }.ToImmutableDictionary());

        _ = fakeCashServiceClient.AdjustSystemV2Async(
            Arg.Is(walletId),
            Arg.Is(-request.Amount),
            Arg.Is(0M),
            Arg.Is(request.Remark),
            Arg.Is(default(string)),
            Arg.Is(userId))
            .Returns(new TransactionResponse
            {
                TransactionId = Guid.NewGuid(),
                Balance = 0M,
                LockedBalance = 0M
            });

        var expectedApplyInfo = new PlayerBalanceAdjustmentApplyInfo(
            brandId,
            playerCode,
            walletId,
            AdjustmentType.Add,
            PlayerAdjustmentTransactionType.SystemAdjust,
            AdjustTransferType.None,
            request.Amount,
            request.BucketAmount,
            request.IncludeInNetProfit,
            request.Remark);

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.increasePlayerAmount(playerCode, request);
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { playerCode, request });

        // Assert
        Assert.True(actual?.Valid);
        _ = fakePlayerAdjustBalanceService.Received(1)
            .ProcessAdjustApplyAsync(
                Arg.Is(expectedApplyInfo),
                Arg.Is(userId),
                Arg.Is("system"),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 玩家調帳扣款()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerAdjustBalanceService = Substitute.For<IPlayerAdjustBalanceService>();

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
                .AddSingleton(fakeCashServiceClient)
                .AddSingleton(fakePlayerAdjustBalanceService))
            .Build();

        await web.StartAsync();

        var playerCode = "test1";
        var request = new CreatePlayerReduceAmountRequest
        {
            Amount = 1000M,
            BucketAmount = 0M,
            IncludeInNetProfit = true,
            Remark = "test",
            TransactionType = PlayerReduceTransactionType.SystemAdjust
        };
        var walletId = Guid.NewGuid();
        var brandId = Guid.NewGuid();

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Tgs.PlayerService.Client.PlayerInfo
            {
                PlayerCode = playerCode,
                UserName = "tester",
                BrandId = brandId
            });

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                ["DefaultWalletId"] = walletId.ToString()
            }.ToImmutableDictionary());

        _ = fakeCashServiceClient.AdjustSystemV2Async(
            Arg.Is(walletId),
            Arg.Is(-request.Amount),
            Arg.Is(0M),
            Arg.Is(request.Remark),
            Arg.Is(default(string)),
            Arg.Is(userId))
            .Returns(new TransactionResponse
            {
                TransactionId = Guid.NewGuid(),
                Balance = 0M,
                LockedBalance = 0M
            });

        var expectedApplyInfo = new PlayerBalanceAdjustmentApplyInfo(
            brandId,
            playerCode,
            walletId,
            AdjustmentType.Subtract,
            PlayerAdjustmentTransactionType.SystemAdjust,
            AdjustTransferType.None,
            request.Amount,
            request.BucketAmount,
            request.IncludeInNetProfit,
            request.Remark);

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.reducePlayerAmount(playerCode, request);
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { playerCode, request });

        // Assert
        Assert.True(actual?.Valid);
        _ = fakePlayerAdjustBalanceService.Received(1)
            .ProcessAdjustApplyAsync(
                Arg.Is(expectedApplyInfo),
                Arg.Is(userId),
                Arg.Is("system"),
                Arg.Any<CancellationToken>());
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
            .Returns(new Tgs.PlayerService.Client.PlayerInfo
            {
                PlayerCode = playerCode,
                BrandId = Guid.NewGuid()
            });

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                ["DefaultWalletId"] = walletId.ToString()
            }.ToImmutableDictionary());

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

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
        await nodeService.InvokeFromStringAsync(
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
