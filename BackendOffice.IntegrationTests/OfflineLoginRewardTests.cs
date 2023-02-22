using System;
using System.Runtime.InteropServices;
using System.Text.Json;
using DocumentFormat.OpenXml.Wordprocessing;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Promotion;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.ApplicationServices.Promotion.OfflineLoginReward;
using TGS.Services.BackOffice.Domain;
using Xunit;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class OfflineLoginRewardTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public OfflineLoginRewardTests(
        ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 建立登入送線下活動()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakePlayerItemsQueryService =
            Substitute.For<IQueryService<OfflineLoginRewardPlayerItemsQuery, OfflineLoginRewardPlayerItem>>();

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
                .AddSingleton(fakePromotionServiceClient)
                .AddSingleton(fakePlayerItemsQueryService))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var promotionId = Guid.NewGuid();

        var from = DateOnly.Parse("2022-02-18");
        var to = DateOnly.Parse("2022-03-06");

        var players = new[]
        {
            new OfflineLoginRewardPlayerItem("player1", "username1"),
            new OfflineLoginRewardPlayerItem("player2", "username2")
        };

        _ = fakePlayerItemsQueryService.QueryAsync(
                Arg.Is(new OfflineLoginRewardPlayerItemsQuery(batchId)),
                Arg.Any<CancellationToken>())
            .Returns(players.ToAsyncEnumerable());

        var createInfo = new CreatePromotionMainInfo
        {
            Name = "Test OfflineLoginReward",
            EventDuring = new During
            {
                From =
                    new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8)).UtcDateTime,
                To = new DateTimeOffset(to.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8)).UtcDateTime
            },
            PromotionType = "OfflineLoginReward",
            IsAutoSent = true,
            PromotionLimitAmount = null
        };
        
        _ = fakePromotionServiceClient.CreatePromotionAsync(
                Arg.Is(brandId),
                Arg.Is(createInfo),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(promotionId);

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.createOfflineLoginRewardPromotion(brandId, request);
            };
        """;

        var request = new CreateOfflineLoginRewardPromotionViewModel
        {
            Name = "Test OfflineLoginReward",
            BatchId = batchId,
            BonusAmount = 20m,
            TurnoverMultiple = 1m,
            From = from,
            To = to
        };

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { brandId, request });

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);

        _ = fakePromotionServiceClient
            .Received(1)
            .SetPromotionRuleAsync(
                Arg.Is(promotionId),
                Arg.Any<JsonDocument>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _ = fakePromotionServiceClient
            .Received(1)
            .SetPromotionStatusAsync(
                Arg.Is(promotionId),
                Arg.Is(PromotionStatus.Enabled),
                Arg.Any<string>(),
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

                return await svc.checkOfflineLoginRewardBackgroundProcessCompleted(batchId);
            };
        """;

        // Act
        await StaticNodeJSService.InvokeFromStringAsync(
            nodeModule,
            args: new object[] { batchId });

        _ = fakeCheckService
            .Received(1)
            .GetAsync(
                Arg.Is(new CheckOfflineLoginRewardBackgroundProcessCompletedQuery(batchId)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 匯出Excel樣板()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeExportService = Substitute.For<IGetService<ExportOfflineLoginRewardFormTemplateCommand, Stream>>();

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

        var command = new ExportOfflineLoginRewardFormTemplateCommand();

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

                return await svc.exportOfflineLoginRewardFormTemplate();
            };
        """;

        // Act
        await StaticNodeJSService.InvokeFromStringAsync(nodeModule);

        _ = fakeExportService
            .Received(1)
            .GetAsync(
                Arg.Is(command),
                Arg.Any<CancellationToken>());
    }
}
