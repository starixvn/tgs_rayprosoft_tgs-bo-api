using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Text.Json;
using ExpectedObjects;
using Jering.Javascript.NodeJS;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using Tgs.VipLevelService.Clients;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Domain;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class PlayerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public PlayerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 只用BrandId查詢玩家列表()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeAgentRepository = Substitute.For<IAgentRepository>();
        var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();

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
                .AddSingleton(fakePlayerServiceClient)
                .AddSingleton(fakePaymentServiceClient)
                .AddSingleton(fakeAgentRepository)
                .AddSingleton(fakeVipLevelServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var page = 1;
        var rows = 20;
        var sortby = "registeredTime";
        var orderby = SortOrder.Ascending;
        var showAll = false;

        _ = fakeAgentRepository.GetAgentsAsync(Arg.Is(brandId))
            .Returns(new AgentInfo[] {
                new AgentInfo(Guid.NewGuid(),"agent1",Guid.NewGuid(),AgentStatus.Enabled),
                new AgentInfo(Guid.NewGuid(),"agent2",Guid.NewGuid(),AgentStatus.Enabled)
            }.ToAsyncEnumerable());

        var vipId1 = Guid.NewGuid();

        _ = fakeVipLevelServiceClient.GetBrandAllVipLevelsAsync(
                Arg.Is(brandId),
                Arg.Any<CancellationToken>())
            .Returns(new VipLevelInfo[] {
                new VipLevelInfo(vipId1,1,brandId, 0M,0M,0M,0M,false,0M,0M,0M,0M,0M,0M,0M,0M,0M),
                new VipLevelInfo(Guid.NewGuid(),2,brandId, 0M,0M,0M,0M,false,0M,0M,0M,0M,0M,0M,0M,0M,0M)
            }.ToAsyncEnumerable());

        _ = fakePlayerServiceClient.QueryPlayerListAsync(
                Arg.Any<Guid>(),
                Arg.Any<PlayerListQuery>(),
                Arg.Any<CancellationToken>())
            .Returns(new PlayerListItem[] {
                new PlayerListItem(
                    PlayerCode: "4ucmqplxb029",
                    UserName: "4ucmqplxb029",
                    RealName: null,
                    RegisteredOn: null,
                    RegisteredDomainName: null,
                    AgentId: null,
                    VipId: vipId1,
                    Status: Tgs.PlayerService.Client.PlayerStatus.Enabled,
                    ParentPlayerCode: null),
                new PlayerListItem(
                    PlayerCode: "4ucmqplxkuyd",
                    UserName: "4ucmqplxkuyd",
                    RealName: null,
                    RegisteredOn:null,
                    RegisteredDomainName:null,
                    AgentId:null,
                    VipId: vipId1,
                    Status:Tgs.PlayerService.Client.PlayerStatus.Enabled,
                    ParentPlayerCode:null)
            }.ToAsyncEnumerable());

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getPlayerList(request);
            };
        """;

        var request = new GetPlayersRequest
        {
            BrandId = brandId,
            OrderBy = orderby,
            Page = page,
            Rows = rows,
            SortBy = sortby,
            ShowAll = showAll
        };

        var expected = new PagedResponse<PlayersModel>()
        {
            TotalCount = 2,
            Valid = true,
            Items = new[] {
                new PlayersModel{
                    VipLevel = 1,
                    PlayerCode = "4ucmqplxb029",
                    Username = "4ucmqplxb029",
                    Status = TGS.Core.Models.Enums.PlayerStatus.Enabled
                },
                new PlayersModel{
                    VipLevel = 1,
                    PlayerCode = "4ucmqplxkuyd",
                    Username = "4ucmqplxkuyd",
                    Status = TGS.Core.Models.Enums.PlayerStatus.Enabled
                }
            }
        };

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { request });

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<PagedResponse<PlayersModel>>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.True(result?.Valid);
        Assert.Equal(2, result?.TotalCount);
        expected.Items.ToExpectedObject().ShouldEqual(result?.Items);
    }

    [Fact]
    public async Task 更新玩家生日資訊()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeMediator = Substitute.For<IMediator>();

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
                .AddSingleton(fakeMediator))
            .Build();

        await web.StartAsync();

        var playerCode = "test1";
        var birthdate = DateOnly.Parse("2023-04-19");

        _ = fakeMediator.Send(Arg.Any<UpdatePlayerRequest>())
            .Returns(new MessageResponse
            {
                Valid = true
            });

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode, birthdate) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.updatePlayer(
                    playerCode,
                    "info",
                    {
                        status: "Enabled",
                        birthdate
                    });
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { playerCode, birthdate.ToString("yyyy/MM/dd") });

        // Assert
        Assert.NotNull(actual);
        //Assert.True(actual.Valid);
    }

    [Fact]
    public async Task 使用BrandId與玩家代碼查詢玩家列表()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeAgentRepository = Substitute.For<IAgentRepository>();
        var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();

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
                .AddSingleton(fakePlayerServiceClient)
                .AddSingleton(fakePaymentServiceClient)
                .AddSingleton(fakeAgentRepository)
                .AddSingleton(fakeVipLevelServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var page = 1;
        var rows = 20;
        var sortby = "registeredTime";
        var orderby = SortOrder.Ascending;
        var showAll = false;

        _ = fakeAgentRepository.GetAgentsAsync(Arg.Is(brandId))
            .Returns(new AgentInfo[] {
                new AgentInfo(Guid.NewGuid(),"agent1",Guid.NewGuid(),AgentStatus.Enabled),
                new AgentInfo(Guid.NewGuid(),"agent2",Guid.NewGuid(),AgentStatus.Enabled)
            }.ToAsyncEnumerable());

        var vipId1 = Guid.NewGuid();

        _ = fakeVipLevelServiceClient.GetBrandAllVipLevelsAsync(
                Arg.Is(brandId),
                Arg.Any<CancellationToken>())
            .Returns(new VipLevelInfo[] {
                new VipLevelInfo(vipId1,1,brandId, 0M,0M,0M,0M,false,0M,0M,0M,0M,0M,0M,0M,0M,0M),
                new VipLevelInfo(Guid.NewGuid(),2,brandId, 0M,0M,0M,0M,false,0M,0M,0M,0M,0M,0M,0M,0M,0M)
            }.ToAsyncEnumerable());

        _ = fakePlayerServiceClient.QueryPlayerListAsync(
                Arg.Is(brandId),
                Arg.Any<PlayerListQuery>(),
                Arg.Any<CancellationToken>())
            .Returns(new PlayerListItem[] {
                new PlayerListItem(
                    PlayerCode: "4ucmqplxb029",
                    UserName: "4ucmqplxb029",
                    RealName: null,
                    RegisteredOn: null,
                    RegisteredDomainName: null,
                    AgentId: null,
                    VipId: vipId1,
                    Status: Tgs.PlayerService.Client.PlayerStatus.Enabled,
                    ParentPlayerCode: null)
            }.ToAsyncEnumerable());

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getPlayerList(request);
            };
        """;

        var request = new GetPlayersRequest
        {
            BrandId = brandId,
            PlayerCode = "4ucmqplxb029",
            OrderBy = orderby,
            Page = page,
            Rows = rows,
            SortBy = sortby,
            ShowAll = showAll
        };

        var expected = new PagedResponse<PlayersModel>()
        {
            TotalCount = 1,
            Valid = true,
            Items = new[] {
                new PlayersModel{
                    VipLevel = 1,
                    PlayerCode = "4ucmqplxb029",
                    Username = "4ucmqplxb029",
                    Status = TGS.Core.Models.Enums.PlayerStatus.Enabled
                }
            }
        };

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { request });
        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<PagedResponse<PlayersModel>>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.True(result?.Valid);
        Assert.Equal(1, result?.TotalCount);
        expected.Items.ToExpectedObject().ShouldEqual(result?.Items);
    }
}
