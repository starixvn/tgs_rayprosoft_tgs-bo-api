using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class OfflinePromotionTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public OfflinePromotionTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 匯出Excel樣板()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeExportService = Substitute.For<IGetService<ExportOfflinePromotionFormTemplateCommand, Stream>>();

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
                .AddSingleton(fakeExportService))
            .Build();

        await web.StartAsync();

        var command = new ExportOfflinePromotionFormTemplateCommand();

        _ = fakeExportService
            .GetAsync(
                Arg.Is(command),
                Arg.Any<CancellationToken>())
            .Returns(new MemoryStream());

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async () =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.downloadOfflinePromotionFormTemplate();
            };
        """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<JsonNode>(nodeModule);

        Assert.NotNull(actual);
        Assert.Equal(200, actual["status"]!.GetValue<int>());
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            actual["contentType"]!.GetValue<string>());
        _ = fakeExportService.Received(1)
            .GetAsync(
                Arg.Is(command),
                Arg.Any<CancellationToken>());
    }
}
