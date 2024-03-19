using System.Runtime.InteropServices;
using System.Text.Json;
using ExpectedObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Payment.Clients;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.ApplicationServices;
using Xunit;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class PlayerPaymentTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    private readonly NodeJsFixture _nodeJsFixture = nodeJsFixture ?? throw new ArgumentNullException(nameof(nodeJsFixture));

    [Fact]
    public async Task 取得玩家金流總覽資料()
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
                    options => options.UserId = userId)
                .Services
                .AddSingleton(fakePaymentServiceClient))
            .Build();

        await web.StartAsync();

        var playerCode = "test1";
        var time = DateTime.UtcNow;

        _ = fakePaymentServiceClient.GetPlayerPaymentOverviewAsync(
            Arg.Is(playerCode),
            Arg.Any<CancellationToken>())
            .Returns(new PaymentOverview(
                10M,
                20M,
                time,
                30M,
                40M,
                time,
                DateOnly.FromDateTime(time),
                50M,
                1,
                60M,
                3));

        var expected = new PaymentOverviewViewModel
        {
            FirstDepositAmount = 10M,
            FirstDepositHandlingFee = 20M,
            FirstDepositOn = time,
            FirstWithdrawAmount = 30M,
            FirstWithdrawHandlingFee = 40M,
            FirstWithdrawOn = time,
            RecordedBy = DateOnly.FromDateTime(time),
            TotalDepositAmount = 50M,
            TotalWithdrawAmount = 60M,
            TotalDepositCount = 1,
            TotalWithdrawCount = 3
        };

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-payment-service.js').default;

            module.exports = async (playerCode) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getPlayerPaymentOverview(playerCode);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<PaymentOverviewViewModel>(
            nodeModule,
            args: [playerCode]);

        // Assert
        expected.ToExpectedObject().ShouldMatch(actual);
    }

    [Fact]
    public async Task 整合測試_取得玩家銀行卡列表()
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
                    options => options.UserId = userId)
                .Services
                .AddSingleton(fakePaymentServiceClient))
            .Build();

        await web.StartAsync();

        var playerCode = "test1";

        _ = fakePaymentServiceClient.GetPlayerBankInfosAsync(
            Arg.Is(playerCode),
            Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new PlayerBankInfo
                {
                    Id = Guid.NewGuid(),
                    PlayerCode = playerCode,
                    PaymentChannelId = Guid.NewGuid(),
                    BankType = "Bank",
                    BrandId = Guid.NewGuid(),
                    Detail = JsonDocument.Parse("{}"),
                    IsDefault = false,
                    UpdatedOn = DateTime.UtcNow,
                    Modifier = "aaa"
                }
            }.ToAsyncEnumerable());

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-payment-service.js').default;

            module.exports = async (playerCode) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.listPlayerBankInfos(playerCode);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<PagedResponse<PlayerBankInfoViewModel>>(
            nodeModule,
            args: [playerCode]);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(1, actual.TotalCount);
        Assert.Equal("aaa", actual.Items[0].Modifier);
    }
}
