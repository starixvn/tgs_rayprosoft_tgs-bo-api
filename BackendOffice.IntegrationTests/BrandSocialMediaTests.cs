using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.BrandCertificate;
using Tgs.BrandCertificate.Clients;
using TGS.Apis.BackendOffice.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class BrandSocialMediaTests(
    ITestOutputHelper testOutputHelper,
    NodeJsFixture nodeJsFixture)
{
    [Fact]
    public async Task 整合測試_取得Brand社群連結設定列表()
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

        var brandId = Guid.NewGuid();

        _ = fakeBrandServiceClient.ListSocialMediaSettingsAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new SocialMediaSetting(
                    Guid.NewGuid(),
                    "Test1",
                    SocialType.Facebook,
                    new Uri("https://localhost"),
                    100),
                new SocialMediaSetting(
                    Guid.NewGuid(),
                    "Test2",
                    SocialType.Facebook,
                    new Uri("https://localhost"),
                    200),
                new SocialMediaSetting(
                    Guid.NewGuid(),
                    "Test3",
                    SocialType.Facebook,
                    new Uri("https://localhost"),
                    300)
            }.ToAsyncEnumerable());

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (brandId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.listBrandSocialMediaSettings(brandId);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: [brandId]);

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<SocialMediaSettingViewModel[]>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.NotNull(result);
        Assert.Equal(3, result.Length);
    }

    [Fact]
    public async Task 整合測試_新增Brand社群連結()
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

        var brandId = Guid.NewGuid();
        var request = new AddSocialMediaSettingViewModel
        {
            Name = "Test",
            Type = SocialType.Facebook,
            Url = new Uri("https://localhost")
        };
        var expectedCreateId = Guid.NewGuid();

        _ = fakeBrandServiceClient.AddSocialMediaSettingAsync(
            Arg.Is(brandId),
            Arg.Any<AddSocialMediaSetting>(),
            Arg.Is("system"),
            Arg.Any<CancellationToken>())
            .Returns(expectedCreateId);

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (brandId, setting) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                let result = await svc.addBrandSocialMediaSetting(brandId, setting);

                return JSON.stringify(result);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<Guid>(
            nodeModule,
            args: [brandId, request]);

        // Assert
        Assert.Equal(expectedCreateId, actual);
    }

    [Fact]
    public async Task 整合測試_設定社群連結排序()
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

        var settingId = Guid.NewGuid();

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (settingId, newOrder) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                await svc.setSocialMediaSettingOrder(settingId, Number(newOrder));
            };
            """;

        // Act
        await nodeService.InvokeFromStringAsync(
            nodeModule,
            args: [settingId, 200.ToString()]);

        // Assert
        _ = fakeBrandServiceClient.Received(1)
            .SetSocialMediaSettingOrderAsync(
                Arg.Is(settingId),
                Arg.Is(200),
                Arg.Is("system"),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 整合測試_刪除一個社群連結設定()
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

        var brandId = Guid.NewGuid();
        var settingId = Guid.NewGuid();

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (brandId, settingId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                await svc.deleteSocialMediaSetting(brandId, settingId);
            };
            """;

        // Act
        await nodeService.InvokeFromStringAsync(
            nodeModule,
            args: [brandId, settingId]);

        // Assert
        _ = fakeBrandServiceClient.Received(1)
            .DeleteSocialMediaSettingAsync(Arg.Is(brandId), Arg.Is(settingId));
    }
}
