using System.Reactive.Linq;
using System.Runtime.InteropServices;
using ExpectedObjects;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.BrandCertificate.Clients;
using Tgs.StationLetterService.Client;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Messages;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class StationLetterTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public StationLetterTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 取得Brand站內信()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeStationLetterClient = Substitute.For<IStationLetterClient>();

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
                .AddSingleton(fakeStationLetterClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                Code = "GPI"
            });

        var letter = new StationLetter(
            Guid.NewGuid(),
            "test1",
            "GPI",
            "test1",
            null,
            "aaabdfasfdasfd",
            null,
            DateTime.UtcNow,
            false,
            false);
        _ = fakeStationLetterClient.ListStationLettersAsync(Arg.Is("GPI"), Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                letter
            }.ToAsyncEnumerable());

        var expected = new StationLetterListItemViewModel
        {
            Id = letter.Id,
            Sender = letter.Sender,
            Subject = letter.Subject,
            SentOn = letter.SentOn
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, page, rows) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getBrandLetters(brandId, page, rows);
            };
        """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<PagedResponse<StationLetterListItemViewModel>>(
            nodeModule,
            args: new object[] { brandId, 1, 10 });

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(1, actual.TotalCount);
        expected.ToExpectedObject().ShouldEqual(actual.Items.First());
    }

    [Fact]
    public async Task 取得分頁站內信()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeStationLetterClient = Substitute.For<IStationLetterClient>();

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
                .AddSingleton(fakeStationLetterClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                Code = "GPI"
            });

        var letters = new[]{
            new StationLetter(
                Guid.NewGuid(),
                "test1",
                "GPI",
                "test1",
                null,
                "aaabdfasfdasfd",
                null,
                DateTime.UtcNow,
                false,
                false),
            new StationLetter(
                Guid.NewGuid(),
                "test2",
                "GPI",
                "test2",
                null,
                "fafdsafasf",
                null,
                DateTime.UtcNow,
                false,
                false),
            new StationLetter(
                Guid.NewGuid(),
                "test3",
                "GPI",
                "test3",
                null,
                "pppppppp",
                null,
                DateTime.UtcNow,
                false,
                false)
        };
        _ = fakeStationLetterClient.ListStationLettersAsync(Arg.Is("GPI"), Arg.Any<CancellationToken>())
            .Returns(letters.ToAsyncEnumerable());

        var expected = new StationLetterListItemViewModel
        {
            Id = letters[1].Id,
            Sender = letters[1].Sender,
            Subject = letters[1].Subject,
            SentOn = letters[1].SentOn
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, page, rows) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getBrandLetters(brandId, page, rows);
            };
        """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<PagedResponse<StationLetterListItemViewModel>>(
            nodeModule,
            args: new object[] { brandId, 2, 1 });

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(3, actual.TotalCount);
        expected.ToExpectedObject().ShouldEqual(actual.Items.First());
    }
}
