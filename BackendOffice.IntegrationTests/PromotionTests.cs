using System.Runtime.InteropServices;
using System.Text.Json;
using ExpectedObjects;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.BrandCertificate.Clients;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Messages.Promotion;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.ApplicationServices.Promotion;
using TGS.Services.BackOffice.Domain;
using Xunit.Abstractions;
using PromotionStatus = Tgs.Promotion.Clients.PromotionStatus;

namespace BackendOffice.IntegrationTests;

public class PromotionTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public PromotionTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 取得活動MainInfo()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

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
                .AddSingleton(fakePromotionServiceClient))
            .Build();

        await web.StartAsync();

        var promotionId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        _ = fakePromotionServiceClient.GetPromotionMainInfoAsync(
            Arg.Is(promotionId),
            Arg.Any<CancellationToken>())
            .Returns(new PromotionMainInfo
            {
                Id = promotionId,
                Name = "TestPromotion",
                BrandId = brandId,
                EventDuring = new During(now.AddDays(-1), now.AddDays(1)),
                PromotionType = "TestPromotion",
                PromotionLimitAmount = null,
                IsAutoSent = true
            });

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (promotionId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getPromotionMainInfo(promotionId);
            };
        """;

        var expected = new PromotionMainInfoViewModel
        {
            Id = promotionId,
            Name = "TestPromotion",
            BrandId = brandId,
            From = now.AddDays(-1),
            To = now.AddDays(1),
            PromotionType = "TestPromotion",
            PromotionLimitAmount = null,
            AutoSentPlayerBonus = true
        };

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<PromotionMainInfoViewModel>(
            nodeModule,
            args: new object[] { promotionId });

        // Assert
        Assert.NotNull(actual);

        expected.ToExpectedObject().ShouldEqual(actual);
    }

    [Fact]
    public async Task 取得活動內容()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

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
                .AddSingleton(fakePromotionServiceClient))
            .Build();

        await web.StartAsync();

        var promotionId = Guid.NewGuid();

        _ = fakePromotionServiceClient.GetPromotionContentAsync(
                Arg.Is(promotionId),
                Arg.Any<CancellationToken>())
            .Returns(new PromotionContent
            {
                SubSubject = "TestSubject",
                Content = null,
                PersonalComputerBanner = "http://www.abc.com/banner.jpg",
                Localization = JsonDocument.Parse("{}")
            });

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (promotionId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getPromotionContent(promotionId);
            };
        """;

        var expected = new PromotionContentViewModel
        {
            SubSubject = "TestSubject",
            Content = null,
            DesktopBanner = "http://www.abc.com/banner.jpg",
            Localization = JsonDocument.Parse("{}")
        };

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<PromotionContentViewModel>(
            nodeModule,
            args: new object[] { promotionId });

        // Assert
        Assert.NotNull(actual);

        expected
            .ToExpectedObject(ctx => ctx.PushStrategy<JsonDocumentComparisionStrategy>())
            .ShouldEqual(actual);
    }

    [Fact]
    public async Task 取得活動規則()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

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
                .AddSingleton(fakePromotionServiceClient))
            .Build();

        await web.StartAsync();

        var promotionId = Guid.NewGuid();

        var rule = new PromotionRuleInfo(
            JsonDocument.Parse(
"""
{
    "bonusAmount": 1000
}
"""),
            1000m,
            10m,
            1m,
            null,
            new[] { 1, 2, 3 });

        _ = fakePromotionServiceClient.GetPromotionRuleAsync<PromotionRuleInfo>(
            Arg.Is(promotionId),
            Arg.Any<CancellationToken>())
            .Returns(rule);

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (promotionId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getPromotionRule(promotionId);
            };
        """;

        var expected = JsonDocument.Parse(
"""
{
    "bonusAmount": 1000,
    "vipLevels": [1,2,3]
}
""");

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<JsonDocument>(
            nodeModule,
            args: new object[] { promotionId });

        // Assert
        Assert.NotNull(actual);

        expected
            .ToExpectedObject(ctx =>
                ctx.PushStrategy<JsonDocumentComparisionStrategy>())
            .ShouldMatch(actual);
    }

    [Fact]
    public async Task 新增活動()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

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
                .AddSingleton(fakePromotionServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var request = new CreatePromotionRequest
        {
            BrandId = brandId,
            Name = "TestPromotion",
            From = DateOnly.Parse("2023-01-01"),
            To = DateOnly.Parse("2023-01-31"),
            PromotionType = "TestPromotion",
            AutoSentPlayerBonus = false,
            Unlimited = true,
            Rule = JsonDocument.Parse("{}"),
            SubSubject = "TestPromotion",
            VipLevels = new[] { 1, 2 },
            Status = SwitchStatus.Enabled
        };

        var promotionId = Guid.NewGuid();

        var from = new DateTimeOffset(request.From.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8)).UtcDateTime;
        var to = new DateTimeOffset(request.To.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8)).UtcDateTime;

        var createInfo = new CreatePromotionMainInfo
        {
            Name = request.Name,
            EventDuring = (from, to),
            PromotionType = request.PromotionType,
            IsAutoSent = request.AutoSentPlayerBonus,
            PromotionLimitAmount = request.Unlimited ? null : request.TotalAmountLimit
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

            module.exports = async (request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.createPromotion(request);
            };
        """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { request });

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);

        _ = fakePromotionServiceClient.Received(1)
            .CreatePromotionAsync(
                Arg.Is(brandId),
                Arg.Is(createInfo),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionRuleAsync(
                Arg.Is(promotionId),
                Arg.Any<JsonDocument>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionContentAsync(
                Arg.Is(promotionId),
                Arg.Any<PromotionContent>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionVipLevelsAsync(
                Arg.Is(promotionId),
                Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(request.VipLevels)),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionStatusAsync(
                Arg.Is(promotionId),
                Arg.Is(Tgs.Promotion.Clients.PromotionStatus.Enabled),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 新增活動MainInfo()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

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
                .AddSingleton(fakePromotionServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();

        var request = new CreatePromotionMainInfoViewModel
        {
            Name = "TestPromotion",
            From = DateOnly.Parse("2023-01-01"),
            To = DateOnly.Parse("2023-01-31"),
            PromotionType = "TestPromotion",
            AutoSentPlayerBonus = false,
            Unlimited = true,
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.createPromotionMainInfo(brandId, request);
            };
        """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { brandId, request });

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);

        var from = new DateTimeOffset(request.From.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8)).UtcDateTime;
        var to = new DateTimeOffset(request.To.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8)).UtcDateTime;

        var createInfo = new CreatePromotionMainInfo
        {
            Name = request.Name,
            EventDuring = (from, to),
            PromotionType = request.PromotionType,
            IsAutoSent = request.AutoSentPlayerBonus,
            PromotionLimitAmount = request.Unlimited ? null : request.TotalAmountLimit
        };

        _ = fakePromotionServiceClient.Received(1)
            .CreatePromotionAsync(
                Arg.Is(brandId),
                Arg.Is(createInfo),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 複製活動()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeGetBrandInfoService = Substitute.For<IGetService<GetBrandInfoQuery, BrandInfo?>>();
        var fakeSystemClock = Substitute.For<ISystemClock>();
        var fakeGetMainInfoService = Substitute.For<IGetService<GetPromotionMainInfoQuery, PromotionMainInfo>>();
        var fakeGetRuleService = Substitute.For<IGetService<GetPromotionRuleQuery, PromotionRuleInfo>>();
        var fakeGetContentService = Substitute.For<IGetService<GetPromotionContentQuery, PromotionContent>>();
        var fakeCreateMainInfoService = Substitute.For<IGetService<CreatePromotionMainInfoCommand, Guid>>();
        var fakeUpdateContentService = Substitute.For<ICommandService<UpdatePromotionContentCommand>>();
        var fakeUpdateRuleService = Substitute.For<ICommandService<UpdatePromotionRuleCommand>>();
        var fakeLogger = NullLogger<DuplicatePromotionCommandService>.Instance;

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
                .AddSingleton(fakeGetBrandInfoService)
                .AddSingleton(fakeGetMainInfoService)
                .AddSingleton(fakeGetRuleService)
                .AddSingleton(fakeGetContentService)
                .AddSingleton(fakeCreateMainInfoService)
                .AddSingleton(fakeUpdateRuleService)
                .AddSingleton(fakeUpdateContentService)
                .AddSingleton(fakeLogger)
                .AddSingleton(fakeSystemClock))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var basePromotionId = Guid.NewGuid();

        _ = fakeGetMainInfoService.GetAsync(
            Arg.Is(new GetPromotionMainInfoQuery(basePromotionId)),
            Arg.Any<CancellationToken>())
            .Returns(new PromotionMainInfo
            {
                Id = basePromotionId,
                BrandId = brandId,
                Name = "TestPromotion",
                EventDuring = new During(
                    DateTime.Parse("2023-03-13 16:00:00"),
                    DateTime.Parse("2023-03-15 16:00:00")),
                IsAutoSent = false,
                PromotionLimitAmount = null,
                PromotionType = "TestPromotion"
            });

        _ = fakeGetRuleService.GetAsync(
            Arg.Is(new GetPromotionRuleQuery(basePromotionId)),
            Arg.Any<CancellationToken>())
            .Returns(new PromotionRuleInfo(
                JsonDocument.Parse("{}"),
                1000m,
                10m,
                1m,
                null,
                new[] { 1, 2, 3 }));

        _ = fakeGetContentService.GetAsync(
            Arg.Is(new GetPromotionContentQuery(basePromotionId)),
            Arg.Any<CancellationToken>())
            .Returns(new PromotionContent
            {
                SubSubject = "TestSubject",
                Localization = JsonDocument.Parse("{}")
            });

        var utcNow = DateTimeOffset.Parse("2023-03-16 16:00:00+00:00");
        _ = fakeSystemClock.UtcNow.Returns(utcNow);

        _ = fakeGetBrandInfoService.GetAsync(
            Arg.Is(new GetBrandInfoQuery(brandId)),
            Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                TimeSpanMinutes = 480
            });

        var createInfoCommand = new CreatePromotionMainInfoCommand(
            brandId,
            "TestPromotion",
            "TestPromotion",
            DateOnly.Parse("2023-03-18"),
            DateOnly.Parse("2023-03-20"),
            true,
            null,
            false,
            "system");

        var expectedPromotionId = Guid.NewGuid();

        _ = fakeCreateMainInfoService.GetAsync(
            Arg.Is(createInfoCommand),
            Arg.Any<CancellationToken>())
            .Returns(expectedPromotionId);

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (basePromotionId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.duplicatePromotion(basePromotionId);
            };
        """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<ResponseViewModel<Guid?>>(
            nodeModule,
            args: new object[] { basePromotionId });

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);
        Assert.Equal(expectedPromotionId, actual.Data);

        _ = fakeUpdateRuleService.Received(1)
            .ExecuteAsync(
                Arg.Is<UpdatePromotionRuleCommand>(x => x.PromotionId == expectedPromotionId),
                Arg.Any<CancellationToken>());

        _ = fakeUpdateContentService.Received(1)
            .ExecuteAsync(
                Arg.Is<UpdatePromotionContentCommand>(x => x.PromotionId == expectedPromotionId),
                Arg.Any<CancellationToken>());

        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionStatusAsync(
                Arg.Is(expectedPromotionId),
                Arg.Is(Tgs.Promotion.Clients.PromotionStatus.Disabled),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 變更活動()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

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
                .AddSingleton(fakePromotionServiceClient))
            .Build();

        await web.StartAsync();

        var request = new UpdatePromotionRequest
        {
            Name = "TestPromotion",
            From = DateOnly.Parse("2023-01-01"),
            To = DateOnly.Parse("2023-01-31"),
            PromotionType = "TestPromotion",
            AutoSentPlayerBonus = false,
            Unlimited = true,
            Rule = JsonDocument.Parse("{}"),
            SubSubject = "TestPromotion",
            VipLevels = new[] { 1, 2 },
            Status = SwitchStatus.Enabled
        };

        var promotionId = Guid.NewGuid();

        _ = fakePromotionServiceClient.GetPromotionMainInfoAsync(
            Arg.Is(promotionId),
            Arg.Any<CancellationToken>())
            .Returns(new PromotionMainInfo
            {
                Id = promotionId,
                BrandId = Guid.NewGuid()
            });

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (promotionId, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.updatePromotion(promotionId, request);
            };
        """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { promotionId, request });

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);

        var from = new DateTimeOffset(request.From.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8)).UtcDateTime;
        var to = new DateTimeOffset(request.To.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8)).UtcDateTime;

        var updateMainInfo = new CreatePromotionMainInfo
        {
            Name = request.Name,
            EventDuring = (from, to),
            PromotionType = request.PromotionType,
            IsAutoSent = request.AutoSentPlayerBonus,
            PromotionLimitAmount = request.Unlimited ? null : request.TotalAmountLimit
        };

        _ = fakePromotionServiceClient.Received(1)
            .UpdatePromotionMainInfoAsync(
                Arg.Is(promotionId),
                Arg.Is(updateMainInfo),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionRuleAsync(
                Arg.Is(promotionId),
                Arg.Any<JsonDocument>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionContentAsync(
                Arg.Is(promotionId),
                Arg.Any<PromotionContent>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionVipLevelsAsync(
                Arg.Is(promotionId),
                Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(request.VipLevels)),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionStatusAsync(
                Arg.Is(promotionId),
                Arg.Is(Tgs.Promotion.Clients.PromotionStatus.Enabled),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 變更活動MainInfo()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

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
                .AddSingleton(fakePromotionServiceClient))
            .Build();

        await web.StartAsync();

        var request = new UpdatePromotionMainInfoViewModel
        {
            Name = "TestPromotion",
            From = DateOnly.Parse("2023-01-01"),
            To = DateOnly.Parse("2023-01-31"),
            PromotionType = "TestPromotion",
            AutoSentPlayerBonus = false,
            Unlimited = true,
        };

        var promotionId = Guid.NewGuid();

        _ = fakePromotionServiceClient.GetPromotionMainInfoAsync(
            Arg.Is(promotionId),
            Arg.Any<CancellationToken>())
            .Returns(new PromotionMainInfo
            {
                Id = promotionId,
                BrandId = Guid.NewGuid()
            });

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (promotionId, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.updatePromotionMainInfo(promotionId, request);
            };
        """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { promotionId, request });

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);

        var from = new DateTimeOffset(request.From.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8)).UtcDateTime;
        var to = new DateTimeOffset(request.To.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8)).UtcDateTime;

        var updateInfo = new CreatePromotionMainInfo
        {
            Name = request.Name,
            EventDuring = (from, to),
            PromotionType = request.PromotionType,
            IsAutoSent = request.AutoSentPlayerBonus,
            PromotionLimitAmount = request.Unlimited ? null : request.TotalAmountLimit
        };

        _ = fakePromotionServiceClient.Received(1)
            .UpdatePromotionMainInfoAsync(
                Arg.Is(promotionId),
                Arg.Is(updateInfo),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 變更活動內容()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

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
                .AddSingleton(fakePromotionServiceClient))
            .Build();

        await web.StartAsync();

        var promotionId = Guid.NewGuid();

        var request = new UpdatePromotionContentViewModel
        {
            SubSubject = "TestPromotion",
            Content = "TestPromotionContent",
            DesktopBanner = "http://abc.com/desktop.jpg",
            Localization = JsonDocument.Parse("{}")
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (promotionId, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.updatePromotionContent(promotionId, request);
            };
        """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { promotionId, request });

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);

        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionContentAsync(
                Arg.Is(promotionId),
                Arg.Is<PromotionContent>(x => x.Content == request.Content),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 變更活動狀態()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

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
                .AddSingleton(fakePromotionServiceClient))
            .Build();

        await web.StartAsync();

        var promotionId = Guid.NewGuid();
        var status = TGS.Core.Models.Enums.PromotionStatus.Enabled;

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (promotionId, status) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.updatePromotionStatus(promotionId, status);
            };
        """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { promotionId, status });

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);

        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionStatusAsync(
                Arg.Is(promotionId),
                Arg.Is(PromotionStatus.Enabled),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 變更活動規則()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

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
                .AddSingleton(fakePromotionServiceClient))
            .Build();

        await web.StartAsync();

        var promotionId = Guid.NewGuid();

        var request = new UpdatePromotionRuleViewModel
        {
            GameProviderIds = new[] { Guid.NewGuid() },
            Rule = JsonDocument.Parse("{}"),
            VipLevels = new[] { 1, 2, 3 }
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (promotionId, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.updatePromotionRule(promotionId, request);
            };
        """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { promotionId, request });

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);

        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionRuleAsync(
                Arg.Is(promotionId),
                Arg.Any<JsonDocument>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionVipLevelsAsync(
                Arg.Is(promotionId),
                Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(request.VipLevels)),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }
}
