using System.Runtime.InteropServices;
using System.Text.Json;
using ExpectedObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.BrandCertificate.Clients;
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
            item => new BrandSmsSettingViewModel
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
        var actual = await nodeService.InvokeFromStringAsync<BrandSmsSettingViewModel[]>(
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

    [Fact]
    public async Task 整合測試_以BrandId查詢Brand簡訊設定()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
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
                .AddSingleton(fakeBrandServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                SmsEnabled = true
            });

        _ = fakeBrandServiceClient.GetBrandSmsSettingsAsync<JsonDocument>(
                Arg.Is(brandId),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new SmsVendorInfo<JsonDocument>("Mock", JsonDocument.Parse("{}")));

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (brandId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getBrandSmsInfo(brandId);
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: [brandId]);

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<BrandSmsResultViewModel>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.True(result?.EnableSms);
        Assert.Equal("Mock", result?.SmsVendorCode);
    }
}