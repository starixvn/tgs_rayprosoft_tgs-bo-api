using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using ExpectedObjects;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.PlayerService.Client;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;
using Valhalla.MessageQueue;
using Xunit;
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
        var clientToken = "token";
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

        var batchId = Guid.NewGuid();

        _ = fakeBatchTempRepository.SaveAdjustBalanceBatchAsync(
            Arg.Any<IEnumerable<BatchAdjustBalanceItem>>(),
            Arg.Is(clientToken),
            Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var items = callInfo.Arg<IEnumerable<BatchAdjustBalanceItem>>();

                Assert.Equal(expected, items);

                return batchId;
            });

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, clientToken, filePath) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.importBatchAdjustForm(brandId, clientToken, filePath);
            };
        """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { brandId, clientToken, importFilePath });

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(batchId, Guid.Parse(actual!));

        _ = fakeBatchTempRepository.Received(1)
            .SaveAdjustBalanceBatchAsync(
                Arg.Any<IEnumerable<BatchAdjustBalanceItem>>(),
                Arg.Is(clientToken),
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

    [Fact]
    public async Task 處理批次調帳()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeMessageSender = Substitute.For<IMessageSender>();

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
                .AddSingleton(fakeMessageSender))
            .Build();

        await web.StartAsync();

        var batchId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var condition = new BatchAdjustBalanceProcessConditionViewModel
        {
            IncludeInNetProfit = true,
            TransactionType = PlayerAdjustmentTransactionType.CompanyAdjust
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, batchId, condition) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.processAdjustBatch(brandId, batchId, condition);
            };
        """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<int>(
            nodeModule, args: new object[] { batchId, brandId, condition });

        // Assert
        Assert.Equal(200, actual);

        _ = fakeMessageSender.Received(1)
            .PublishAsync(
                Arg.Is("tgs.bo.background.adjust_batch"),
                Arg.Any<ReadOnlyMemory<byte>>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 預覽批次調帳清單()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

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
                .AddSingleton(fakeBatchTempRepository))
            .Build();

        await web.StartAsync();

        var batchId = Guid.NewGuid();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (batchId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.previewBatchAdjustBatchData(batchId);
            };
        """;

        var batchData = new[]
        {
            new BatchAdjustBalanceItem(
                1,
                Guid.NewGuid(),
                Guid.NewGuid(),
                "test1",
                "Test1",
                100,
                100,
                "r1"),
            new BatchAdjustBalanceItem(
                2,
                Guid.NewGuid(),
                Guid.NewGuid(),
                "test2",
                "Test2",
                200,
                200,
                "r1")
        };

        _ = fakeBatchTempRepository.GetAdjustBalanceBatchAsync(
            Arg.Is(batchId),
            Arg.Any<CancellationToken>())
            .Returns(batchData.ToAsyncEnumerable());

        var expected = batchData.Select(data => new BatchAdjustBalanceItemViewModel
        {
            Id = data.Id,
            UserName = data.UserName,
            AdjustAmount = data.AdjustAmount,
            Turnover = data.Turnover,
            Remark = data.Remark
        }).ToArray();

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<BatchAdjustBalanceItemViewModel[]>(
            nodeModule,
            args: new object[] { batchId });

        // Assert
        expected.ToExpectedObject().ShouldMatch(actual);
    }

    [Fact]
    public async Task 檢查調帳批次是否已經完成()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

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
                .AddSingleton(fakeBatchTempRepository))
            .Build();

        await web.StartAsync();

        var batchId = Guid.NewGuid();

        _ = fakeBatchTempRepository.GetBatchIsCompletedAsync(
            Arg.Is(batchId),
            Arg.Any<CancellationToken>()).Returns(true);

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (batchId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.checkAdjustBatchIsCompleted(batchId);
            };
        """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<int>(
            nodeModule, args: new object[] { batchId });

        // Assert
        Assert.Equal(200, actual);
    }

    [Fact]
    public async Task 取得批次調帳處理結果()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

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
                .AddSingleton(fakeBatchTempRepository))
            .Build();

        await web.StartAsync();

        var batchId = Guid.NewGuid();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (batchId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getBatchAdjustProcessResult(batchId);
            };
        """;

        var batchData = new[]
        {
            new BatchAdjustBalanceItemResult(
                1,
                true),
            new BatchAdjustBalanceItemResult(
                2,
                true)
        };

        _ = fakeBatchTempRepository.GetAdjustBalanceItemResultsAsync(
            Arg.Is(batchId),
            Arg.Any<CancellationToken>())
            .Returns(batchData.ToAsyncEnumerable());

        var expected = batchData.Select(data => new BatchAdjustBalanceItemResultViewModel
        {
            Id = data.Id,
            IsSuccess = data.IsSuccess
        }).ToArray();

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<BatchAdjustBalanceItemResultViewModel[]>(
            nodeModule,
            args: new object[] { batchId });

        // Assert
        expected.ToExpectedObject().ShouldMatch(actual);
    }
}
