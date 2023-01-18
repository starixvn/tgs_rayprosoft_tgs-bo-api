using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.ServiceUtility;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.ApplicationServices;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class PlayerIpTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public PlayerIpTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 用戶查詢IP()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeService = Substitute.For<IGetService<GetPlayerIpsByUserNameQuery, PagedResponse<GetPlayerIpsByUserNameResult>>>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
            .ConfigureServices(services => services
                .AddAuthentication("Fake")
                .AddScheme<FakeAuthenticationSchemaOptions, FakeAuthenticationHandler>(
                    "Fake",
                    options => options.UserId = userId)
                .Services
                .AddSingleton(fakeService))
        .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var userName = "test1";
        var page = 1;
        var rows = 20;
        var sortby = "createdOn";
        var orderby = SortOrder.Descending;
        var showAll = false;

        var query = new GetPlayerIpsByUserNameQuery
        {
            BrandId = brandId,
            UserName = userName,
            Page = page,
            Rows = rows,
            SortBy = sortby,
            OrderBy = orderby,
            ShowAll = showAll
        };
        var response = new PagedResponse<GetPlayerIpsByUserNameResult>()
        {
            TotalCount = 2,
            Valid = true,
            Items = new GetPlayerIpsByUserNameResult[] {
                new GetPlayerIpsByUserNameResult{
                    IpAddress="111.90.145.159",
                    CreatedOn=DateTimeOffset.Parse("2022-12-27T05:38:00.15534+00:00") },
                new GetPlayerIpsByUserNameResult{
                    IpAddress="114.33.41.237",
                    CreatedOn=DateTimeOffset.Parse("2022-12-28T10:36:38.562382+00:00") }
            }
        };
        _ = fakeService.GetAsync(
            query: query,
            cancellationToken: Arg.Any<CancellationToken>())
        .Returns(response);

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, userName, page, rows, sortby, orderby, showAll) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                svc.getPlayerIpsByUserName(brandId, userName, page, rows, sortby, orderby, showAll);
            };
        """;

        // Act
        await StaticNodeJSService.InvokeFromStringAsync(
            nodeModule,
            args: new object[] { brandId, userName, page, rows, sortby, orderby, showAll });
    }
}
