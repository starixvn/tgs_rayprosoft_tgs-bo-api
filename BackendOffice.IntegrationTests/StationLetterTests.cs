using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using ExpectedObjects;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.BrandCertificate.Clients;
using Tgs.PlayerService.Client;
using Tgs.StationLetterService;
using Tgs.StationLetterService.Client;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Messages;
using Xunit;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class StationLetterTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public StationLetterTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 回覆玩家站內信()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeStationLetterClient = Substitute.For<IStationLetterClient>();

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
                .AddSingleton(fakeBrandServiceClient)
                .AddSingleton(fakeStationLetterClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var letterId = Guid.NewGuid();
        var content = "reply";

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                Code = "GPI"
            });

        _ = fakeStationLetterClient.GetStationLetterAsync(Arg.Is(letterId), Arg.Any<CancellationToken>())
            .Returns(new StationLetter(
                letterId,
                "test1",
                "GPI",
                "aa",
                null,
                "bb",
                null,
                DateTime.UtcNow,
                false,
                false));

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, letterId, content) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.replyBrandLetter(brandId, letterId, content);
            };
            """;

        // Act
        await StaticNodeJSService.InvokeFromStringAsync(
            nodeModule,
            args: new object[] { brandId, letterId, content });

        // Assert
        _ = fakeStationLetterClient.Received(1)
            .ReplyStationLetterAsync(
                Arg.Is(letterId),
                Arg.Is(new ReplyStationLetterRequest("GPI", "test1", content)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 取得Brand站內信()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeStationLetterClient = Substitute.For<IStationLetterClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

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
                .AddSingleton(fakeBrandServiceClient)
                .AddSingleton(fakeStationLetterClient)
                .AddSingleton(fakePlayerServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                Code = "GPI"
            });

        var letter = new StationLetter(
            Guid.NewGuid(),
            "test1",
            "GPI",
            "test1",
            null,
            "aaabdfasfdasfd",
            null,
            DateTime.UtcNow,
            false,
            false);
        _ = fakeStationLetterClient.ListStationLettersAsync(Arg.Is("GPI"), Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                letter
            }.ToAsyncEnumerable());

        _ = fakePlayerServiceClient.QueryPlayerInfoListAsync(
            Arg.Is(brandId),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(new[] {
                new PlayerInfo
                {
                    PlayerCode = "test1",
                    UserName = "Player1"
                }
            }.ToAsyncEnumerable());

        var expected = new StationLetterListItemViewModel
        {
            Id = letter.Id,
            Sender = "Player1",
            Subject = letter.Subject,
            SentOn = letter.SentOn
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, page, rows) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getBrandLetters(brandId, null, page, rows);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<PagedResponse<StationLetterListItemViewModel>>(
            nodeModule,
            args: new object[] { brandId, 1, 10 });

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(1, actual.TotalCount);
        expected.ToExpectedObject().ShouldEqual(actual.Items.First());
    }

    [Fact]
    public async Task 取得分頁站內信()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeStationLetterClient = Substitute.For<IStationLetterClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

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
                .AddSingleton(fakeBrandServiceClient)
                .AddSingleton(fakeStationLetterClient)
                .AddSingleton(fakePlayerServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                Code = "GPI"
            });

        var letters = new[]{
            new StationLetter(
                Guid.NewGuid(),
                "test1",
                "GPI",
                "test1",
                null,
                "aaabdfasfdasfd",
                null,
                DateTime.UtcNow,
                false,
                false),
            new StationLetter(
                Guid.NewGuid(),
                "test2",
                "GPI",
                "test2",
                null,
                "fafdsafasf",
                null,
                DateTime.UtcNow,
                false,
                false),
            new StationLetter(
                Guid.NewGuid(),
                "test3",
                "GPI",
                "test3",
                null,
                "pppppppp",
                null,
                DateTime.UtcNow,
                false,
                false)
        };
        _ = fakeStationLetterClient.ListStationLettersAsync(Arg.Is("GPI"), Arg.Any<CancellationToken>())
            .Returns(letters.ToAsyncEnumerable());

        _ = fakePlayerServiceClient.QueryPlayerInfoListAsync(
            Arg.Is(brandId),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(new[] {
                new PlayerInfo
                {
                    PlayerCode = "test2",
                    UserName = "Player2"
                }
            }.ToAsyncEnumerable());

        var expected = new StationLetterListItemViewModel
        {
            Id = letters[1].Id,
            Sender = "Player2",
            Subject = letters[1].Subject,
            SentOn = letters[1].SentOn
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, page, rows) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getBrandLetters(brandId, null, page, rows);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<PagedResponse<StationLetterListItemViewModel>>(
            nodeModule,
            args: new object[] { brandId, 2, 1 });

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(3, actual.TotalCount);
        expected.ToExpectedObject().ShouldEqual(actual.Items.First());
    }

    [Fact]
    public async Task 依照PlayerCode過濾站內信列表()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeStationLetterClient = Substitute.For<IStationLetterClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

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
                .AddSingleton(fakeBrandServiceClient)
                .AddSingleton(fakeStationLetterClient)
                .AddSingleton(fakePlayerServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var searchPlayerCode = "test2";

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                Code = "GPI"
            });

        var letters = new[]{
            new StationLetter(
                Guid.NewGuid(),
                "test1",
                "GPI",
                "test1",
                null,
                "aaabdfasfdasfd",
                null,
                DateTime.UtcNow,
                false,
                false),
            new StationLetter(
                Guid.NewGuid(),
                "test2",
                "GPI",
                "test2",
                null,
                "fafdsafasf",
                null,
                DateTime.UtcNow,
                false,
                false),
            new StationLetter(
                Guid.NewGuid(),
                "test3",
                "GPI",
                "test3",
                null,
                "pppppppp",
                null,
                DateTime.UtcNow,
                false,
                false)
        };
        _ = fakeStationLetterClient.ListStationLettersAsync(Arg.Is("GPI"), Arg.Any<CancellationToken>())
            .Returns(letters.ToAsyncEnumerable());

        _ = fakePlayerServiceClient.QueryPlayerInfoListAsync(
            Arg.Is(brandId),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(new[] {
                new PlayerInfo
                {
                    PlayerCode = "test2",
                    UserName = "Player2"
                }
            }.ToAsyncEnumerable());

        var expected = new StationLetterListItemViewModel
        {
            Id = letters[1].Id,
            Sender = "Player2",
            Subject = letters[1].Subject,
            SentOn = letters[1].SentOn
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, playerCode, page, rows) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getBrandLetters(brandId, playerCode, page, rows);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<PagedResponse<StationLetterListItemViewModel>>(
            nodeModule,
            args: new object[] { brandId, searchPlayerCode, 1, 10 });

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(1, actual.TotalCount);
        expected.ToExpectedObject().ShouldEqual(actual.Items.First());
    }

    [Fact]
    public async Task 取得分頁寄件匣列表()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeStationLetterClient = Substitute.For<IStationLetterClient>();

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
                .AddSingleton(fakeBrandServiceClient)
                .AddSingleton(fakeStationLetterClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                Code = "GPI"
            });

        var letters = new[]{
            new DeliveryLetter(
                Guid.NewGuid(),
                "test1",
                ReceiverType.Receivers,
                JsonDocument.Parse("""["GPI"]"""),
                "test1",
                "aaabdfasfdasfd",
                null,
                DateTime.UtcNow),
            new DeliveryLetter(
                Guid.NewGuid(),
                "tes2",
                ReceiverType.Receivers,
                JsonDocument.Parse("""["GPI"]"""),
                "test2",
                "fafdsafasf",
                null,
                DateTime.UtcNow),
            new DeliveryLetter(
                Guid.NewGuid(),
                "test3",
                ReceiverType.Receivers,
                JsonDocument.Parse("""["GPI"]"""),
                "test3",
                "pppppppp",
                null,
                DateTime.UtcNow)
        };
        _ = fakeStationLetterClient.ListDeliverLettersAsync(Arg.Is("GPI"), Arg.Any<CancellationToken>())
            .Returns(letters.ToAsyncEnumerable());

        var expected = new
        {
            letters[1].Id,
            ReceiverType = letters[1].ReceiverType.ToString(),
            letters[1].Subject,
            letters[1].SentOn
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, page, rows) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getDeliveryLetters(brandId, page, rows);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<PagedResponse<DeliveryLetterListItemViewModel>>(
            nodeModule,
            args: new object[] { brandId, 2, 1 });

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(3, actual.TotalCount);
        expected.ToExpectedObject().ShouldMatch(actual.Items.First());
    }

    [Fact]
    public async Task 取得站內信內容()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeStationLetterClient = Substitute.For<IStationLetterClient>();

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
                .AddSingleton(fakeBrandServiceClient)
                .AddSingleton(fakeStationLetterClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var letterId = Guid.NewGuid();

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                Code = "GPI"
            });

        var letter = new StationLetter(
            letterId,
            "test1",
            "GPI",
            "ttt",
            null,
            "abcdee",
            null,
            DateTime.UtcNow,
            false,
            false);
        _ = fakeStationLetterClient.GetStationLetterAsync(Arg.Is(letterId), Arg.Any<CancellationToken>())
            .Returns(letter);

        var expected = new StationLetterViewModel
        {
            Id = letterId,
            Sender = letter.Sender,
            Content = letter.Content,
            HaveRead = letter.HaveRead,
            Receiver = letter.Receiver,
            Replied = letter.Replied,
            SentOn = letter.SentOn,
            Subject = letter.Subject,
            Quotation = letter.Quotation
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, letterId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getBrandLetter(brandId, letterId);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<StationLetterViewModel>(
            nodeModule,
            args: new object[] { brandId, letterId });

        // Assert
        Assert.NotNull(actual);
        expected.ToExpectedObject().ShouldEqual(actual);
    }

    [Fact]
    public async Task 取得寄件備份內容()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeStationLetterClient = Substitute.For<IStationLetterClient>();

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
                .AddSingleton(fakeBrandServiceClient)
                .AddSingleton(fakeStationLetterClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                Code = "GPI"
            });

        var letter = new DeliveryLetter(
            deliveryId,
            "GPI",
            ReceiverType.Receivers,
            JsonDocument.Parse("""["test1"]"""),
            "ttt",
            "abcdee",
            null,
            DateTime.UtcNow);

        _ = fakeStationLetterClient.GetDeliveryLetterAsync(Arg.Is(deliveryId), Arg.Any<CancellationToken>())
            .Returns(letter);

        var expected = new
        {
            Id = deliveryId,
            letter.Sender,
            letter.Content,
            ReceiverType = letter.ReceiverType.ToString(),
            letter.SentOn,
            letter.Subject
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, deliveryId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getDeliveryLetter(brandId, deliveryId);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<DeliveryLetterViewModel>(
            nodeModule,
            args: new object[] { brandId, deliveryId });

        // Assert
        Assert.NotNull(actual);
        expected.ToExpectedObject().ShouldMatch(actual);
    }

    [Fact]
    public async Task 傳送站內信給玩家()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeStationLetterClient = Substitute.For<IStationLetterClient>();

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
                .AddSingleton(fakeBrandServiceClient)
                .AddSingleton(fakeStationLetterClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();

        var request = new CreateStationLetterViewModel
        {
            Receivers = new[] { "test1" },
            Subject = "ttt",
            Content = "abcde"
        };

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                Code = "GPI"
            });

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.sendBrandLetter(brandId, request);
            };
            """;

        // Act
        await StaticNodeJSService.InvokeFromStringAsync(
            nodeModule,
            args: new object[] { brandId, request });

        // Assert
        var requestExpected = new StationLetterRequest(
            "GPI",
            request.Receivers,
            request.Subject,
            request.Content).ToExpectedObject(ctx =>
            {
                _ = ctx.IgnoreEqualsOverride();
                _ = ctx.ArrayMemberShouldEqual(o => o.Receivers);
            });
        _ = fakeStationLetterClient.Received(1)
            .SendStationLetterAsync(
                Arg.Is<StationLetterRequest>(req => requestExpected.Equals(req)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 傳送站內信給全部玩家()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeStationLetterClient = Substitute.For<IStationLetterClient>();

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
                .AddSingleton(fakeBrandServiceClient)
                .AddSingleton(fakeStationLetterClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();

        var request = new CreateAllPlayersStationLetterViewModel
        {
            Subject = "ttt",
            Content = "abcde"
        };

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                Code = "GPI"
            });

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.sendAllPlayersBrandLetter(brandId, request);
            };
            """;

        // Act
        await StaticNodeJSService.InvokeFromStringAsync(
            nodeModule,
            args: new object[] { brandId, request });

        // Assert
        var requestExpected = new BrandAllPlayersStationLetterRequest(
            "GPI",
            request.Subject,
            request.Content);

        _ = fakeStationLetterClient.Received(1)
            .SendBrandAllPlayersStationLetterAsync(
                Arg.Is(requestExpected),
                Arg.Any<CancellationToken>());
    }
}
