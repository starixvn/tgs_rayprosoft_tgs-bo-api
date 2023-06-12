using System.Runtime.InteropServices;
using ExpectedObjects;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.BrandCertificate.Clients;
using Tgs.CashQueryService.Client;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class AdjustWalletTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public AdjustWalletTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 取得調帳統計資料()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeCashQueryClient = Substitute.For<ICashQueryClient>();
        var fakeBrandRelationQueryService = Substitute.For<IQueryService<BrandRelationQuery, BrandRelationInfo>>();

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
                .AddSingleton(fakeCashQueryClient)
                .AddSingleton(fakeBrandRelationQueryService))
            .Build();

        await web.StartAsync();

        var (from, to) = (DateTime.Now.AddDays(-1D), DateTime.Now);
        var brandId1 = Guid.NewGuid();
        var brandCode1 = "Test1";
        var brandId2 = Guid.NewGuid();
        var brandCode2 = "Test2";
        var licenseeId = Guid.NewGuid();
        var licensee = "L1";

        var expectedCondition = new AdjustRecordQueryCondition(
            (from.ToUniversalTime(), to.ToUniversalTime()),
            null,
            null,
            null);

        _ = fakeCashQueryClient.QueryPlayerAdjustSummariesAsync(Arg.Is(expectedCondition))
            .Returns(new[] {
                new AdjustSummaryListItem(
                    brandId1,
                    1,
                    2,
                    3,
                    4,
                    5,
                    0),
                new AdjustSummaryListItem(
                    brandId2,
                    5,
                    4,
                    3,
                    2,
                    1,
                    0)
            }.ToAsyncEnumerable());
        _ = fakeCashQueryClient.QueryAgentAdjustSummariesAsync(Arg.Is(expectedCondition))
            .Returns(new[]
            {
                new AdjustSummaryListItem(
                    brandId1,
                    0,
                    0,
                    0,
                    0,
                    0,
                    7),
            }.ToAsyncEnumerable());
        fakeBrandRelationQueryService.QueryAsync(Arg.Any<BrandRelationQuery>())
            .Returns(new[] {
                new BrandRelationInfo(
                    brandId1,
                    brandCode1,
                    brandCode1,
                    licenseeId,
                    licensee,
                    licensee,
                    Guid.NewGuid(),
                    string.Empty,
                    string.Empty),
                new BrandRelationInfo(
                    brandId2,
                    brandCode2,
                    brandCode2,
                    licenseeId,
                    licensee,
                    licensee,
                    Guid.NewGuid(),
                    string.Empty,
                    string.Empty)
            }.ToAsyncEnumerable());

        var expected = new[]{
            new AdjustSummaryListItemViewModel
            {
                BrandId = brandId1,
                Brand = brandCode1,
                Licensee = licensee,
                Company = 1,
                Agent = 2,
                System = 3,
                Transfer = 4,
                Bonus = 5,
                AgentOther = 7
            },
            new AdjustSummaryListItemViewModel
            {
                BrandId = brandId2,
                Brand = brandCode2,
                Licensee = licensee,
                Company = 5,
                Agent = 4,
                System = 3,
                Transfer = 2,
                Bonus = 1,
                AgentOther = 0
            }
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (from, to) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.queryAdjustSummaries(from, to);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<AdjustSummaryListItemViewModel[]>(
            nodeModule,
            args: new object[] { from, to });

        // Assert
        expected.ToExpectedObject().ShouldEqual(actual);
    }
}
