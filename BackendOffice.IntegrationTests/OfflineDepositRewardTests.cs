using System.Runtime.InteropServices;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.BrandCertificate.Clients;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;
using TGS.Services.BackOffice.Domain;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class OfflineDepositRewardTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public OfflineDepositRewardTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 完成上傳後以BatchId建立線下儲值活動()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakePlayerItemsQueryService = Substitute.For<IGetService<GetProcessedUsersQuery, OfflineUsersResult>>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

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
                .AddSingleton(fakePlayerItemsQueryService)
                .AddSingleton(fakeBrandServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();

        var request = new CreateOfflineDepositRewardPromotionViewModel
        {
            Name = "OfflineDeposit",
            BatchId = Guid.NewGuid(),
            TurnoverMultiple = 1m,
            From = DateOnly.Parse("2022-02-18"),
            To = DateOnly.Parse("2022-03-06"),
            Phases = new[] {
                new OfflineDepositPhaseViewModel
                {
                    Condition = 100M,
                    Bonus = 20M
                },
                new OfflineDepositPhaseViewModel
                {
                    Condition = 500M,
                    Bonus = 50M
                }
            }
        };

        _ = fakeBrandServiceClient.GetBrandInfoAsync(Arg.Is(brandId))
            .Returns(new BrandInfo
            {
                Id = brandId,
                TimeSpanMinutes = 480
            });

        var players = new[]
        {
            new OfflinePlayerItem("player1", "username1"),
            new OfflinePlayerItem("player2", "username2")
        };

        _ = fakePlayerItemsQueryService.GetAsync(
            Arg.Is(new GetProcessedUsersQuery(request.BatchId)),
            Arg.Any<CancellationToken>())
            .Returns(new OfflineUsersResult(players, 2, Array.Empty<string>()));

        var promotionId = Guid.NewGuid();
        var createInfo = new CreatePromotionMainInfo
        {
            Name = request.Name,
            EventDuring = (new DateTimeOffset(request.From.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8D)).UtcDateTime,
                new DateTimeOffset(request.To.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8D)).UtcDateTime),
            PromotionType = "OfflineDepositReward",
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

                return await svc.createOfflineDepositRewardPromotion(brandId, request);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { brandId, request });

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);

        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionRuleAsync(
                Arg.Is(promotionId),
                Arg.Any<OfflineDepositSettings>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionStatusAsync(
                Arg.Is(promotionId),
                Arg.Is(PromotionStatus.Enabled),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }
}
