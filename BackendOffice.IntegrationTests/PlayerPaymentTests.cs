using System.Runtime.InteropServices;
using ExpectedObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Payment.Clients;
using TGS.Apis.BackendOffice.ViewModels;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class PlayerPaymentTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly NodeJsFixture _nodeJsFixture;

    public PlayerPaymentTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
        _nodeJsFixture = nodeJsFixture ?? throw new ArgumentNullException(nameof(nodeJsFixture));
    }

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
                10,
                20,
                time,
                30,
                40,
                time,
                DateOnly.FromDateTime(time),
                50,
                60));

        var expected = new PaymentOverviewViewModel
        {
            FirstDepositAmount = 10,
            FirstDepositHandlingFee = 20,
            FirstDepositOn = time,
            FirstWithdrawAmount = 30,
            FirstWithdrawHandlingFee = 40,
            FirstWithdrawOn = time,
            RecordedBy = DateOnly.FromDateTime(time),
            TotalDepositAmount = 50,
            TotalWithdrawAmount = 60
        };

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode, aaa) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getPlayerPaymentOverview(playerCode);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<PaymentOverviewViewModel>(
            nodeModule,
            args: new object[] { playerCode });

        // Assert
        expected.ToExpectedObject().ShouldMatch(actual);
    }
}
