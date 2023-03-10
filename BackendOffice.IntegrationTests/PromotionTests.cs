using System.Runtime.InteropServices;
using System.Text.Json;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Promotion.Clients;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Messages;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class PromotionTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public PromotionTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 更新活動內容()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        
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
                .AddSingleton(fakePromotionServiceClient))
            .Build();

        await web.StartAsync();

        var promotionId = Guid.NewGuid();

        var request = new UpdatePromotionContentViewModel
        {
            SubSubject = "TestPromotion", 
            Content = "TestPromotionContent",
            DesktopBanner = "http://abc.com/desktop.jpg",
            Localization = JsonDocument.Parse("{}")
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (promotionId, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.updatePromotionContent(promotionId, request);
            };
        """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { promotionId, request });
        
        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);

        _ = fakePromotionServiceClient
            .Received(1)
            .SetPromotionContentAsync(
                Arg.Is(promotionId),
                Arg.Is<PromotionContent>(x => x.Content == request.Content),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }
}