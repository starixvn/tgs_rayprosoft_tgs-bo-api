using System.Runtime.InteropServices;
using System.Text.Json;
using ExpectedObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.SmsService.Client;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class SmsSettingTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
{
    [Fact]
    public async Task 整合測試_建立簡訊設定()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeSmsServiceClientClient = Substitute.For<ISmsServiceClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
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
                .AddSingleton(fakeSmsServiceClientClient))
            .Build();

        await web.StartAsync();

        var request = new AddSmsSettingRequest
        {
            Name = "name",
            Code = "code",
            Setting = "{}",
        };

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.addSmsSetting(request);
            };
        """;

        // Act
        _ = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: [request]);

        // Assert
        _ = fakeSmsServiceClientClient.Received(1)
            .AddSmsSettingAsync(
                Arg.Is(request.Name),
                Arg.Is(request.Code),
                Arg.Any<JsonDocument>(),
                Arg.Is("system"),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 整合測試_刪除簡訊設定()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeSmsServiceClientClient = Substitute.For<ISmsServiceClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
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
                .AddSingleton(fakeSmsServiceClientClient))
            .Build();

        await web.StartAsync();

        var smsSettingId = Guid.NewGuid();

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (smsSettingId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.removeSmsSetting(smsSettingId);
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: [smsSettingId]);

        // Assert
        _ = fakeSmsServiceClientClient
            .Received(1)
            .RemoveSmsSettingAsync(
                Arg.Is(smsSettingId),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 整合測試_編輯簡訊設定()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeSmsServiceClient = Substitute.For<ISmsServiceClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
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
                .AddSingleton(fakeSmsServiceClient))
            .Build();

        await web.StartAsync();

        var smsSettingId = Guid.NewGuid();
        var request = new UpdateSmsSettingRequest
        {
            Name = "name",
            Code = "code",
            Setting = "{}"
        };

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (smsSettingId, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.updateSmsSetting(smsSettingId, request);
            };
        """;

        // Act
        _ = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: [smsSettingId, request]);

        // Assert
        _ = fakeSmsServiceClient.Received(1)
            .UpdateSmsSettingAsync(
                Arg.Is(smsSettingId),
                Arg.Is(request.Name),
                Arg.Is(request.Code),
                Arg.Any<JsonDocument>(),
                Arg.Is("system"),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 整合測試_查詢全部簡訊設定()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeSmsServiceClient = Substitute.For<ISmsServiceClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
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
                .AddSingleton(fakeSmsServiceClient))
            .Build();

        await web.StartAsync();        

        var rows = new[]
        {
            new SmsSettingInfo(
                Guid.NewGuid(),
                "name1",
                "code1",
                JsonDocument.Parse("{}")),
            new SmsSettingInfo(
                Guid.NewGuid(),
                "name2",
                "code2",
                JsonDocument.Parse("{}"))
        };

        var expected = rows.Select(
            item => new SmsSettingViewModel
            {
                SmsSettingId = item.SmsSettingId,
                Name = item.Name,
                Code = item.Code,
                Setting = item.Setting,
            })
            .ToArray();

        _ = fakeSmsServiceClient
            .QuerySmsSettingListAsync(Arg.Any<CancellationToken>())
            .Returns(rows.ToAsyncEnumerable());

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async () =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getSmsSettings();
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<SmsSettingViewModel[]>(
            nodeModule,
            args: []);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(2, actual.Length);
        expected
            .ToExpectedObject(ctx => ctx.PushStrategy<JsonDocumentComparisionStrategy>())
            .ShouldEqual(actual);
    }
}