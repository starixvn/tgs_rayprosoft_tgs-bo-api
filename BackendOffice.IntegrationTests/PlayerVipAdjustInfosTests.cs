using System.Data.SqlClient;
using System.Runtime.InteropServices;
using ExpectedObjects;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.ServiceUtility;
using Tgs.VipLevelService.Clients;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.ApplicationServices.Players;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class PlayerVipAdjustInfosTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly NodeJsFixture _nodeJsFixture;

    public PlayerVipAdjustInfosTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
        _nodeJsFixture = nodeJsFixture ?? throw new ArgumentNullException(nameof(nodeJsFixture));
    }

    [Fact]
    public async Task 查詢玩家VIP升降紀錄()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeService = Substitute.For<IGetService<GetPlayerVipAdjustQuery, PagingResponse<PlayerVipAdjustInfo>>>();

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

        var playerCode = "4ucmqplxb029";
        var brandId = Guid.NewGuid();
        var from = new DateTimeOffset(2022, 12, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var page = 1;
        var rows = 20;
        var sortby = "CreatedOn";
        var orderby = SortOrder.Descending;
        var showAll = false;
        var pageRequest = new PageableRequest
        {
            Rows = rows,
            Page = page,
            SortBy = sortby,
            OrderBy = SortOrder.Descending,
            ShowAll = showAll,
        };

        var paging = new Paging()
        {
            Page = page,
            Rows = rows,
            IsDesc = true,
            ShowAll = showAll,
            SortBy = sortby,
        };

        var response = new PagingResponse<PlayerVipAdjustInfo>()
        {
            TotalCount = 2,
            Items = new PlayerVipAdjustInfo[] {
                new PlayerVipAdjustInfo{
                    OldLevel = 1,
                    NewLevel = 2,
                    CreatedOn = DateTime.Parse("2022-12-27T05:38:00.15534+00:00") },
                new PlayerVipAdjustInfo{
                    OldLevel = 2,
                    NewLevel = 1,
                    CreatedOn = DateTime.Parse("2022-12-28T10:36:38.562382+00:00") }
            }
        };

        _ = fakeService.GetAsync(
            query: Arg.Any<GetPlayerVipAdjustQuery>(),
            cancellationToken: Arg.Any<CancellationToken>())
        .Returns(response);

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode, brandId, from, to, page, rows, sortby, orderby, showAll) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getVipAdjustInfos(playerCode, brandId, from, to, page, rows, sortby, orderby, showAll);
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<PagedResponse<PlayerVipAdjustInfo>>(
            nodeModule,
            args: new object[] { playerCode, brandId, from, to, page, rows, sortby, orderby, showAll });

        //Assert
        Assert.Equal(2, actual!.TotalCount);
        response.Items.ToExpectedObject().ShouldMatch(actual!.Items);
    }
}
