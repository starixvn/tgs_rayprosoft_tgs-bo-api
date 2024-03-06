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
using TGS.Apis.BackendOffice.Controllers.Messages.BrandMaintenance;
using TGS.Core.Models.Messages;
using Valhalla.MessageQueue;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class BrandMaintenanceTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    private readonly NodeJsFixture _nodeJsFixture = nodeJsFixture ?? throw new ArgumentNullException(nameof(nodeJsFixture));

    [Fact]
    public async Task 變更所有Brand為維護狀態()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

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
                .AddSingleton(fakeBrandServiceClient))
            .Build();

        await web.StartAsync();

        var brands = new[]
        {
            new BrandListItem(
                Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
                BrandStatus.Active,
                null,
                null,
                DateTime.Parse("2023-01-01 10:00:00"),
                DateTime.Parse("2023-01-01 10:00:00")),
        new BrandListItem(
            Guid.Parse("9f1ecf19-38cd-474f-b0d2-93638dc07d21"),
            BrandStatus.Active,
            null,
            null,
            DateTime.Parse("2023-01-10 10:00:00"),
            DateTime.Parse("2023-01-10 10:00:00"))
        };

        _ = fakeBrandServiceClient.QueryBrandListAsync(
                Arg.Is<BrandListQuery>(x => x.Status == BrandStatus.Active),
                Arg.Any<CancellationToken>())
            .Returns(brands.ToAsyncEnumerable());

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async () =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.setAllBrandsMaintenance();
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(nodeModule);

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);

        _ = fakeBrandServiceClient.Received(2)
            .UpdateBrandStatusAsync(
                Arg.Any<Guid>(),
                Arg.Is(BrandStatus.Maintenance),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 變更Brand維護時間()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeMessageSender = Substitute.For<IMessageSender>();

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
                .AddSingleton(fakeMessageSender))
            .Build();

        await web.StartAsync();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        var request = new UpdateBrandMaintenanceTimeRequest
        {
            BeginTime = TimeOnly.Parse("02:00"),
            EndTime = TimeOnly.Parse("02:30")
        };

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (brandId, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.setBrandMaintenanceTime(brandId, request);
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: [
                brandId,
                request
            ]);

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);

        _ = fakeBrandServiceClient.Received(1)
            .UpdateBrandMaintenanceTimeAsync(
                Arg.Is(brandId),
                Arg.Is(request.BeginTime),
                Arg.Is(request.EndTime),
                Arg.Any<CancellationToken>());

        _ = fakeMessageSender.Received(1)
            .PublishAsync(
                Arg.Is("tgs.brandservice.maintenance.cache.clear"),
                Arg.Any<byte[]>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 取得Brands維護資訊()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

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
                .AddSingleton(fakeBrandServiceClient))
            .Build();

        await web.StartAsync();

        var request = new GetBrandMaintenanceInfosRequest
        {
            BrandCode = "TGS1112"
        };

        var infos = new BrandMaintenanceInfo[]
        {
            new() {
                BeginTime = "02:00:00",
                BrandCode = "TGS1112",
                BrandId = Guid.Parse("210a79bb-76a9-4e6a-94b3-5ba26b9a63f0"),
                BrandMaintenanceId = Guid.Parse("ec6cfb63-98c2-44a0-a7ab-6e4715b67596"),
                BrandName = "TGS1112",
                BrandStatus = BrandStatus.Active,
                EndTime = "02:30:00",
                LicenseeName = "Turnkey Game System"
            }
        };

        _ = fakeBrandServiceClient.GetBrandMaintenanceInfosAsync(
                Arg.Is(request.BrandCode),
                Arg.Any<CancellationToken>())
            .Returns(infos.ToAsyncEnumerable());

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-brand-service.js').default;

            module.exports = async (request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getBrandMaintenanceInfos(request);
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: [
                request
            ]);

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        
        var result = JsonSerializer.Deserialize<TGS.Services.BackOffice.Domain.BrandMaintenanceInfo[]>(
            actual,
            jsonOptions.JsonSerializerOptions);

        Assert.Equal(request.BrandCode, result![0].BrandCode);
    }
}
