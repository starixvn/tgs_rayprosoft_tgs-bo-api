using System.Data.SqlClient;
using System.Runtime.InteropServices;
using ExpectedObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.ServiceUtility;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportMarket;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class MarketAgentProfitTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly NodeJsFixture _nodeJsFixture;

    public MarketAgentProfitTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
        _nodeJsFixture = nodeJsFixture ?? throw new ArgumentNullException(nameof(nodeJsFixture));
    }

    [Fact]
    public async Task Get_Market_Agent_Profit_Report()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeService = Substitute.For<IGetService<GetMarketAgentProfitsReportRequest, PagedResponse<MarketAgentProfitReportModel>>>();

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
                .AddSingleton(fakeService))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var date = DateTimeOffset.Now;
        var page = 1;
        var rows = 20;
        var sortby = "MasterAgentName";
        var orderby = SortOrder.Descending;
        var showAll = false;

        var request = new GetMarketAgentProfitsReportRequest
        {
            BrandId = brandId,
            Date = date,
            Page = page,
            Rows = rows,
            SortBy = sortby,
            OrderBy = orderby,
            ShowAll = showAll
        };
        var response = new PagedResponse<MarketAgentProfitReportModel>()
        {
            TotalCount = 1,
            Valid = true,
            Items = new[] {
                new MarketAgentProfitReportModel{
                    MasterAgentName="agentName",
                    Percentage=10M }
            }
        };
        _ = fakeService.GetAsync(
            Arg.Any<GetMarketAgentProfitsReportRequest>(),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(response);

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getMarketAgentProfitsReport(request);
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<PagedResponse<MarketAgentProfitReportModel>>(
            nodeModule,
            args: new object[] { request });

        // Assert
        Assert.True(actual!.Valid);
        Assert.Equal(1, actual!.TotalCount);
        response.Items.ToExpectedObject().ShouldMatch(actual!.Items);
    }
}
