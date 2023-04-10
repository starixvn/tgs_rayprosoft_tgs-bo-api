using System.Runtime.InteropServices;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.BrandCertificate;
using Tgs.BrandCertificate.Clients;
using TGS.Core.Models.Messages;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class BrandMaintenanceTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public BrandMaintenanceTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

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
        
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async () =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.setAllBrandsMaintenance();
            };
        """;
        
        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<MessageResponse>(nodeModule);

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
}
