using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.BrandCertificate.Clients;
using Tgs.PlayerService.Client;
using TGS.Apis.BackendOffice.ViewModels;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class BrandPlayerContractTests(
    ITestOutputHelper testOutputHelper,
    NodeJsFixture nodeJsFixture)
{
    [Fact]
    public async Task 整合測試_取得聯繫信息種類()
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

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async () =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.listPlayerContractType();
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: [brandId]);

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<PlayerPropertyName[]>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.NotNull(result);
        Assert.Equal(8, result.Length);
    }

    [Fact]
    public async Task 整合測試_取得Brand聯繫信息設定列表()
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

        _ = fakeBrandServiceClient.ListBrandPlayerContractDisplayAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new BrandPlayerContractDisplayInfo
                {
                    IsVisualized = true,
                    Name = "Qq"
                },
                new BrandPlayerContractDisplayInfo
                {
                    IsVisualized = true,
                    Name = "Wechat"
                }
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

                return await svc.listBrandPlayerContractDisplay(brandId);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: [brandId]);

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<BrandPlayerContractDisplayViewModel[]>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
    }

    [Fact]
    public async Task 整合測試_設定Brand聯繫信息()
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
        var request = new UpsertBrandPlayerContractRequest()
        {
            { "Qq", true }
        };

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (brandId, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                await svc.setBrandPlayerContractDisplay(brandId, request);
            };
            """;

        // Act
        await nodeService.InvokeFromStringAsync(
            nodeModule,
            args: [brandId, request]);

        // Assert
        _ = fakeBrandServiceClient.SetBrandPlayerContractDisplayAsync(
            Arg.Is(brandId),
            Arg.Is(request.First().Key),
            Arg.Is(request.First().Value), "system",
            Arg.Any<CancellationToken>());
    }
}