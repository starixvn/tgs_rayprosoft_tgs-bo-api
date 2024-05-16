using System.Runtime.InteropServices;
using ExpectedObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.SmsService.Client;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class BrandSmsTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
{
    [Fact]
    public async Task 整合測試_新增Brand簡訊設定()
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

        var brandId = Guid.NewGuid();
        var smsSettingId = Guid.NewGuid();

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (brandId, smsSettingId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.addBrandSms(brandId, smsSettingId);
            };
        """;

        // Act
        await nodeService.InvokeFromStringAsync(
             nodeModule,
             args: [brandId, smsSettingId]);

        // Assert
        _ = fakeSmsServiceClient.Received(1)
            .AddBrandSmsAsync(
                Arg.Is(brandId),
                Arg.Is(smsSettingId),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 整合測試_移除Brand簡訊設定()
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

        var brandId = Guid.NewGuid();
        var smsSettingId = Guid.NewGuid();

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (brandId, smsSettingId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.removeBrandSms(brandId, smsSettingId);
            };
        """;

        // Act
        await nodeService.InvokeFromStringAsync(
             nodeModule,
             args: [brandId, smsSettingId]);

        // Assert
        _ = fakeSmsServiceClient.Received(1)
            .RemoveBrandSmsAsyncAsync(
                Arg.Is(brandId),
                Arg.Is(smsSettingId),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 整合測試_查詢Brand簡訊設定()
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

        var brandId = Guid.NewGuid();

        var rows = new[]
        {
            new BrandSmsInfo(
                Guid.NewGuid(),
                "name1"),
            new BrandSmsInfo(
                Guid.NewGuid(),
                "name2"),
        };

        var expected = rows.Select(
            item => new BrandSmsViewModel
            {
                SmsSettingId = item.SmsSettingId,
                Name = item.SmsSettingName,
            })
            .ToArray();

        _ = fakeSmsServiceClient
            .QueryBrandSmsListAsync(Arg.Is(brandId), Arg.Any<CancellationToken>())
            .Returns(rows.ToAsyncEnumerable());

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (brandId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.listBrandSms(brandId);
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<BrandSmsViewModel[]>(
             nodeModule,
             args: [brandId]);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(2, actual.Length);
        expected
            .ToExpectedObject(ctx => ctx.PushStrategy<JsonDocumentComparisionStrategy>())
            .ShouldEqual(actual);
    }

    [Fact]
    public async Task 整合測試_套用Brand簡訊設定()
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

        var brandId = Guid.NewGuid();
        var smsSettingId = Guid.NewGuid();

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (brandId, smsSettingId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.applyBrandSms(brandId, smsSettingId);
            };
        """;

        // Act
        _ = await nodeService.InvokeFromStringAsync<string>(
             nodeModule,
             args: [brandId, smsSettingId]);

        // Assert
        _ = fakeSmsServiceClient.Received(1)
            .ApplyBrandSmsAsync(
                Arg.Is(brandId),
                Arg.Is(smsSettingId),
                Arg.Is("system"),
                Arg.Any<CancellationToken>());
    }
}