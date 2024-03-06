using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.ServiceUtility;
using Tgs.VipLevelService.Clients;
using Tgs.VipLevelService.Common.ViewModels;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices.Players;
using TGS.Services.BackOffice.Domain;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class PlayerVipAdjustInfosTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    private readonly NodeJsFixture _nodeJsFixture = nodeJsFixture ?? throw new ArgumentNullException(nameof(nodeJsFixture));

    [Fact]
    public async Task 查詢玩家VIP升降紀錄()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeService = Substitute.For<IGetService<GetPlayerVipAdjustQuery, PagingResponseViewModel<PlayerVipLevelLogInfoViewModel>>>();

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
        var orderby = TGS.Core.Models.Messages.SortOrder.Descending;
        var showAll = false;
        var pageRequest = new PageableRequest
        {
            Rows = rows,
            Page = page,
            SortBy = sortby,
            OrderBy = TGS.Core.Models.Messages.SortOrder.Descending,
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

        var response = new PagingResponseViewModel<PlayerVipLevelLogInfoViewModel>()
        {
            TotalCount = 2,
            Items = [
                new(
                    Type:Tgs.VipLevelService.Common.PlayerVipLevelLogType.Register,
                    BeforeLevelId:Guid.Empty,
                    BeforeLevel : 0,
                    BeforeAutoState:Tgs.VipLevelService.Common.PlayerVipLevelAutoState.Enable,
                    AfterLevelId:Guid.NewGuid(),
                    AfterLevel : 1,
                    AfterAutoState:Tgs.VipLevelService.Common.PlayerVipLevelAutoState.Enable,
                    CreatedOn : DateTime.UtcNow,
                    CreatedUserId:Guid.NewGuid(),
                    CreatedUsername:string.Empty,
                    Remark:string.Empty) ,
                new(
                    Type:Tgs.VipLevelService.Common.PlayerVipLevelLogType.System,
                    BeforeLevelId:Guid.NewGuid(),
                    BeforeLevel : 1,
                    BeforeAutoState:Tgs.VipLevelService.Common.PlayerVipLevelAutoState.Enable,
                    AfterLevelId:Guid.NewGuid(),
                    AfterLevel : 2,
                    AfterAutoState:Tgs.VipLevelService.Common.PlayerVipLevelAutoState.Enable,
                    CreatedOn : DateTime.UtcNow,
                    CreatedUserId:Guid.NewGuid(),
                    CreatedUsername:string.Empty,
                    Remark:string.Empty) ,
            ]
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
        var actual = await nodeService.InvokeFromStringAsync<PagedResponse<PlayerVipAdjustViewModel>>(
            nodeModule,
            args: [playerCode, brandId, from, to, page, rows, sortby, orderby, showAll]);

        //Assert
        Assert.Equal(2, actual!.TotalCount);
    }
}
