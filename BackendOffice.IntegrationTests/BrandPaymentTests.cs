using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.Payment.Clients;
using TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;
using TGS.Core.Models.Messages;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class BrandPaymentTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly NodeJsFixture _nodeJsFixture;

    public BrandPaymentTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
        _nodeJsFixture = nodeJsFixture ?? throw new ArgumentNullException(nameof(nodeJsFixture));
    }

    [Fact]
    public async Task 以BrandId查詢Brand自動出款設定()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

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
                .AddSingleton(fakePaymentServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        _ = fakePaymentServiceClient.GetBrandAutoWithdrawSettingAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new AutoWithdrawInfo
            {
                AmountUpperLimit = 100M,
                BrandPaymentIds = Array.Empty<Guid>(),
                EnableAutoWithdraw = true
            });

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getBrandAutoWithdrawSetting(brandId);
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { brandId });

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<AutoWithdrawInfo>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.Equal(100M, result!.AmountUpperLimit);
        Assert.True(result!.EnableAutoWithdraw);
    }

    [Fact]
    public async Task 建立Brand自動出款設定()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

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
                .AddSingleton(fakePaymentServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();

        var request = new UpsertBrandAutoWithdrawRequest
        {
            AmountUpperLimit = 10m,
            BrandPaymentIds = new[] { Guid.NewGuid() },
            EnableAutoWithdraw = true,
        };

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId,request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.upsertBrandAutoWithdraw(brandId,request);
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { brandId, request });

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);
    }
}
