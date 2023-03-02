using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using ExpectedObjects;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class OfflinePromotionTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public OfflinePromotionTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 上傳名單()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeReadService = Substitute.For<IQueryService<ReadOfflinePromotionUserListQuery, string>>();
        var fakeSaveService = Substitute.For<IGetService<SaveOfflineTempUserBatchCommand, Guid>>();
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
                .AddSingleton(fakeReadService)
                .AddSingleton(fakeSaveService)
                .AddSingleton(fakeMessageSender))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var userNames = new[] { "username1", "username2" };

        var batchId = Guid.NewGuid();

        _ = fakeReadService.QueryAsync(
                Arg.Any<ReadOfflinePromotionUserListQuery>(),
                Arg.Any<CancellationToken>())
            .Returns(userNames.ToAsyncEnumerable());

        _ = fakeSaveService.GetAsync(
            Arg.Any<SaveOfflineTempUserBatchCommand>(),
            Arg.Any<CancellationToken>())
            .Returns(batchId);

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, filePath) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.uploadOfflineUserList(brandId, filePath);
            };
        """;

        var expected = new ResponseViewModel<Guid?>
        {
            Valid = true,
            Data = batchId
        };

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/TestOfflineLoginReward.xlsx");

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<ResponseViewModel<Guid?>>(
            nodeModule,
            args: new object[] { brandId, filePath });

        expected.ToExpectedObject().ShouldEqual(actual);

        _ = fakeMessageSender.Received(1)
            .PublishAsync(
                Arg.Is("tgs.bo.background.offline-promotion"),
                Arg.Any<ReadOnlyMemory<byte>>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 匯出Excel樣板()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeExportService = Substitute.For<IGetService<ExportOfflinePromotionFormTemplateCommand, Stream>>();

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
                .AddSingleton(fakeExportService))
            .Build();

        await web.StartAsync();

        var command = new ExportOfflinePromotionFormTemplateCommand();

        _ = fakeExportService
            .GetAsync(
                Arg.Is(command),
                Arg.Any<CancellationToken>())
            .Returns(new MemoryStream());

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async () =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.downloadOfflinePromotionFormTemplate();
            };
        """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<JsonNode>(nodeModule);

        Assert.NotNull(actual);
        Assert.Equal(200, actual["status"]!.GetValue<int>());
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            actual["contentType"]!.GetValue<string>());
        _ = fakeExportService.Received(1)
            .GetAsync(
                Arg.Is(command),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 檢查背景執行狀態()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeCheckService =
            Substitute.For<IGetService<CheckOfflineLoginRewardBackgroundProcessCompletedQuery, bool?>>();

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
                .AddSingleton(fakeCheckService))
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

                return await svc.checkOfflineBackgroundProcessIsCompleted(batchId);
            };
        """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<int>(
            nodeModule,
            args: new object[] { batchId });

        Assert.Equal(204, actual);
        _ = fakeCheckService.Received(1)
            .GetAsync(
                Arg.Is(new CheckOfflineLoginRewardBackgroundProcessCompletedQuery(batchId)),
                Arg.Any<CancellationToken>());
    }
}
