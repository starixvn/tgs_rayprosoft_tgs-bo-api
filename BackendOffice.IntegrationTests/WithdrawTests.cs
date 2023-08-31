using System.Runtime.InteropServices;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Queries.Withdraw;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class WithdrawTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public WithdrawTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 匯出提款單列表()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeWithdrawQueryService = Substitute.For<IGetService<WithdrawFormQuery, WithdrawForms>>();
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
                .AddSingleton(fakeWithdrawQueryService)
                .AddSingleton(fakePaymentServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var (from, to) = (DateTime.Now.AddDays(-1D), DateTime.Now);
        var paymentMethodId = Guid.NewGuid();

        _ = fakeWithdrawQueryService.GetAsync(
            Arg.Any<WithdrawFormQuery>(),
            Arg.Any<CancellationToken>())
            .Returns(new WithdrawForms
            {
                TotalCount = 1,
                Page = 1,
                Rows = 10,
                Sum = 100M,
                Items = new[]
                {
                    new WithdrawFormInfo
                    {
                        Id = Guid.NewGuid(),
                        Amount = 100M,
                        BrandId = brandId
                    }
                }
            });

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, request, paging) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.exportWithdraw(brandId, request, paging);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] {
                brandId,
                new object(),
                new PageableRequest()
            });

        // Assert
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", actual);
    }
}
