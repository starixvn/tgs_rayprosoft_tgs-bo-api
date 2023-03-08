using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.PlayerService.Client;
using TGS.Services.BackOffice.Domain;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class BatchAdjustBalanceTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public BatchAdjustBalanceTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 上傳批次調帳資料()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().Location);
        var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
        var dirPath = Path.GetDirectoryName(codeBasePath)!;

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeBatchTempRepository = Substitute.For<IBatchTempRepository>();

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
                .AddSingleton(fakeBatchTempRepository))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var importFilePath = Path.Combine(dirPath, "Assets", "batch-adj-balance-test1.xlsx");
        var walletId1 = Guid.NewGuid();
        var walletId2 = Guid.NewGuid();
        var walletId3 = Guid.NewGuid();

        _ = fakePlayerServiceClient.GetPlayerInfoByUserNameAsync(
            Arg.Is(brandId),
            Arg.Is("test1"),
            Arg.Any<CancellationToken>())
            .Returns(new PlayerInfo
            {
                PlayerCode = "code1",
                BrandId = brandId,
                UserName = "test1"
            });
        _ = fakePlayerServiceClient.GetPlayerInfoByUserNameAsync(
            Arg.Is(brandId),
            Arg.Is("test2"),
            Arg.Any<CancellationToken>())
            .Returns(new PlayerInfo
            {
                PlayerCode = "code2",
                BrandId = brandId,
                UserName = "test2"
            });
        _ = fakePlayerServiceClient.GetPlayerInfoByUserNameAsync(
            Arg.Is(brandId),
            Arg.Is("test3"),
            Arg.Any<CancellationToken>())
            .Returns(new PlayerInfo
            {
                PlayerCode = "code3",
                BrandId = brandId,
                UserName = "test3"
            });

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(
            Arg.Is("code1"),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                ["DefaultWalletId"] = walletId1.ToString()
            }.ToImmutableDictionary());
        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(
            Arg.Is("code2"),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                ["DefaultWalletId"] = walletId2.ToString()
            }.ToImmutableDictionary());
        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(
            Arg.Is("code3"),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                ["DefaultWalletId"] = walletId3.ToString()
            }.ToImmutableDictionary());

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, filePath) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.importBatchAdjustForm(brandId, filePath);
            };
        """;

        var expected = new[]
        {
            new BatchAdjustBalanceItem(
                0,
                brandId,
                walletId1,
                "code1",
                "test1",
                10M,
                100M,
                string.Empty),
            new BatchAdjustBalanceItem(
                1,
                brandId,
                walletId2,
                "code2",
                "test2",
                11M,
                101M,
                string.Empty),
            new BatchAdjustBalanceItem(
                2,
                brandId,
                walletId3,
                "code3",
                "test3",
                12M,
                102M,
                string.Empty)
        };

        fakeBatchTempRepository.When(
            fake => fake.SaveAdjustBalanceBatchAsync(
                Arg.Any<IEnumerable<BatchAdjustBalanceItem>>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>()))
            .Do(callInfo =>
            {
                var items = callInfo.Arg<IEnumerable<BatchAdjustBalanceItem>>();

                Assert.Equal(expected, items);
            });

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { brandId, importFilePath });

        // Assert
        _ = fakeBatchTempRepository.Received(1)
            .SaveAdjustBalanceBatchAsync(
                Arg.Any<IEnumerable<BatchAdjustBalanceItem>>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 取得上傳檔案範本()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

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
                    }))
            .Build();

        await web.StartAsync();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async () =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.downloadBatchAdjustFormTemplate();
            };
        """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<JsonNode>(
            nodeModule);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(200, actual["status"]!.GetValue<int>());
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            actual["contentType"]!.GetValue<string>());
    }
}
