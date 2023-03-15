using System.Runtime.InteropServices;
using System.Text.Json;
using ExpectedObjects;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Promotion.Clients;
using TGS.Apis.BackendOffice.Controllers.Messages.Promotion;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using Tgs.Promotion;
using TGS.Services.BackOffice.Domain;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class PromotionTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public PromotionTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
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

        _ = fakePromotionServiceClient
            .Received(1)
            .CreatePromotionAsync(
                Arg.Is(brandId),
                Arg.Is(createInfo),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _ = fakePromotionServiceClient
            .Received(1)
            .SetPromotionRuleAsync(
                Arg.Is(promotionId),
                Arg.Any<JsonDocument>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _ = fakePromotionServiceClient
            .Received(1)
            .SetPromotionContentAsync(
                Arg.Is(promotionId),
                Arg.Any<PromotionContent>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _ = fakePromotionServiceClient
            .Received(1)
            .SetPromotionVipLevelsAsync(
                Arg.Is(promotionId),
                Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(request.VipLevels)),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _ = fakePromotionServiceClient
            .Received(1)
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

        _ = fakePromotionServiceClient
            .Received(1)
            .CreatePromotionAsync(
                Arg.Is(brandId),
                Arg.Is(createInfo),
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

        _ = fakePromotionServiceClient
            .Received(1)
            .UpdatePromotionMainInfoAsync(
                Arg.Is(promotionId),
                Arg.Is(updateMainInfo),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _ = fakePromotionServiceClient
            .Received(1)
            .SetPromotionRuleAsync(
                Arg.Is(promotionId),
                Arg.Any<JsonDocument>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _ = fakePromotionServiceClient
            .Received(1)
            .SetPromotionContentAsync(
                Arg.Is(promotionId),
                Arg.Any<PromotionContent>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _ = fakePromotionServiceClient
            .Received(1)
            .SetPromotionVipLevelsAsync(
                Arg.Is(promotionId),
                Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(request.VipLevels)),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _ = fakePromotionServiceClient
            .Received(1)
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

        _ = fakePromotionServiceClient
            .Received(1)
            .SetPromotionContentAsync(
                Arg.Is(promotionId),
                Arg.Is<PromotionContent>(x => x.Content == request.Content),
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

        _ = fakePromotionServiceClient
            .Received(1)
            .SetPromotionRuleAsync(
                Arg.Is(promotionId),
                Arg.Any<JsonDocument>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _ = fakePromotionServiceClient
            .Received(1)
            .SetPromotionVipLevelsAsync(
                Arg.Is(promotionId),
                Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(request.VipLevels)),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
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
}
