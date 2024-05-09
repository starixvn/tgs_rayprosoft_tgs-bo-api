using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text.Json;
using ExpectedObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.Agent.Client;
using Tgs.CashService.Client;
using Tgs.GameProviders.Pluggable.Client;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;
using Tgs.VipLevelService.Clients;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.ApplicationServices.Players;
using TGS.Services.BackOffice.Domain;
using Xunit.Abstractions;
using static TGS.Services.BackOffice.Application.Models.Player.PlayerModel;

namespace BackendOffice.IntegrationTests;

public class PlayerTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    private readonly NodeJsFixture _nodeJsFixture = nodeJsFixture ?? throw new ArgumentNullException(nameof(nodeJsFixture));

    [Fact]
    public async Task 只用BrandId查詢玩家列表()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePaymentServiceClient = Substitute.For<ICashServiceClient>();
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
        var orderby = TGS.Core.Models.Messages.SortOrder.Ascending;
        var showAll = false;

        _ = fakeAgentRepository.GetAgentsAsync(Arg.Is(brandId))
            .Returns(new AgentInfo[] {
                new(Guid.NewGuid(),"agent1",Guid.NewGuid(),AgentStatus.Enabled),
                new(Guid.NewGuid(),"agent2",Guid.NewGuid(),AgentStatus.Enabled)
            }.ToAsyncEnumerable());

        var vipId1 = Guid.NewGuid();

        _ = fakePlayerServiceClient.QueryPlayerListAsync(
                Arg.Any<Guid>(),
                Arg.Any<PlayerListQuery>(),
                Arg.Any<CancellationToken>())
            .Returns(new PlayerListItem[] {
                new(
                    PlayerCode: "4ucmqplxb029",
                    UserName: "4ucmqplxb029",
                    RealName: null,
                    RegisteredOn: null,
                    RegisteredDomainName: null,
                    AgentId: null,
                    VipId: vipId1,
                    Status: Tgs.PlayerService.Client.PlayerStatus.Enabled,
                    ParentPlayerCode: null),
                new(
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

        _ = fakeVipLevelServiceClient.PlayerVipLevelGetAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(new Tgs.VipLevelService.Common.ViewModels.PlayerVipLevelInfoViewModel(
                Guid.NewGuid(),
                "Demo",
                Guid.NewGuid(),
                "TGS",
                Guid.NewGuid(),
                "GPI",
                "4ucmqplxb029",
                "4ucmqplxb029",
                vipId1,
                1,
                Tgs.VipLevelService.Common.PlayerVipLevelAutoState.Enable,
                Guid.NewGuid(),
                0.0m,
                Guid.NewGuid(),
                0.0m,
                99,
                49,
                false,
                Guid.NewGuid(),
                2),
                new Tgs.VipLevelService.Common.ViewModels.PlayerVipLevelInfoViewModel(
                Guid.NewGuid(),
                "Demo",
                Guid.NewGuid(),
                "TGS",
                Guid.NewGuid(),
                "GPI",
                "4ucmqplxkuyd",
                "4ucmqplxkuyd",
                vipId1,
                1,
                Tgs.VipLevelService.Common.PlayerVipLevelAutoState.Enable,
                Guid.NewGuid(),
                0.0m,
                Guid.NewGuid(),
                0.0m,
                99,
                49,
                false,
                Guid.NewGuid(),
                2));

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

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
            Items = [
                new PlayersModel
                {
                    VipLevel = 1,
                    PlayerCode = "4ucmqplxb029",
                    Username = "4ucmqplxb029",
                    Status = TGS.Core.Models.Enums.PlayerStatus.Enabled
                },
                new PlayersModel
                {
                    VipLevel = 1,
                    PlayerCode = "4ucmqplxkuyd",
                    Username = "4ucmqplxkuyd",
                    Status = TGS.Core.Models.Enums.PlayerStatus.Enabled
                }
            ]
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: [request]);

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

        _ = fakeMediator.Send(Arg.Any<UpdatePlayerRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse
            {
                Valid = true
            });

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

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
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: [playerCode, birthdate.ToString("yyyy/MM/dd")]);

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);
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
        var orderby = TGS.Core.Models.Messages.SortOrder.Ascending;
        var showAll = false;

        _ = fakeAgentRepository.GetAgentsAsync(Arg.Is(brandId))
            .Returns(new AgentInfo[] {
                new(Guid.NewGuid(),"agent1",Guid.NewGuid(),AgentStatus.Enabled),
                new(Guid.NewGuid(),"agent2",Guid.NewGuid(),AgentStatus.Enabled)
            }.ToAsyncEnumerable());

        var vipId1 = Guid.NewGuid();

        _ = fakePlayerServiceClient.QueryPlayerListAsync(
                Arg.Is(brandId),
                Arg.Any<PlayerListQuery>(),
                Arg.Any<CancellationToken>())
            .Returns(new PlayerListItem[] {
                new(
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

        _ = fakeVipLevelServiceClient.PlayerVipLevelGetAsync(
            Arg.Is("4ucmqplxb029"),
            Arg.Any<CancellationToken>())
            .Returns(new Tgs.VipLevelService.Common.ViewModels.PlayerVipLevelInfoViewModel(
                Guid.NewGuid(),
                "Demo",
                Guid.NewGuid(),
                "TGS",
                Guid.NewGuid(),
                "GPI",
                "4ucmqplxb029",
                "4ucmqplxb029",
                vipId1,
                1,
                Tgs.VipLevelService.Common.PlayerVipLevelAutoState.Enable,
                Guid.NewGuid(),
                0.0m,
                Guid.NewGuid(),
                0.0m,
                99,
                49,
                false,
                Guid.NewGuid(),
                2));

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

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
            Items = [
                new PlayersModel
                {
                    VipLevel = 1,
                    PlayerCode = "4ucmqplxb029",
                    Username = "4ucmqplxb029",
                    Status = TGS.Core.Models.Enums.PlayerStatus.Enabled
                }
            ]
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: [request]);
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

    [Fact]
    public async Task 依據PlayerCode查詢Player資料()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePaymentSerivceClient = Substitute.For<IPaymentServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
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
                .AddSingleton(fakePaymentSerivceClient)
                .AddSingleton(fakePlayerBucketClient)
                .AddSingleton(fakeCashServiceClient)
                .AddSingleton(fakeVipLevelServiceClient))
            .Build();

        await web.StartAsync();

        var playerCode = "4ucmqplxb029";
        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var vipLevelId = Guid.Parse("bcd5a809-8b90-ec11-a746-ac82478e2af1");
        var NextVipLevelId = Guid.NewGuid();
        _ = fakePlayerServiceClient.GetPlayerInfoAsync(
            Arg.Is(playerCode),
            Arg.Any<CancellationToken>())
            .Returns(new Tgs.PlayerService.Client.PlayerInfo
            {
                PlayerCode = playerCode,
                IdentityAuthentication = true,
                BrandId = brandId,
                IsSetPayPassword = true,
                EmailAuthentication = true,
                PhoneAuthentication = true,
                RegisteredOn = null,
                UserName = "4ucmqplxb029",
                VipLevelId = vipLevelId
            });

        _ = fakePlayerServiceClient.GetPlayerLoginInfoAsync(
            Arg.Is(playerCode),
            Arg.Any<CancellationToken>())
            .Returns(new PlayerLoginInfo(null, null, null, null));

        _ = fakePlayerServiceClient.GetParentPlayerInfoAsync(
            Arg.Is(playerCode),
            Arg.Any<CancellationToken>())
            .Returns(new Tgs.PlayerService.Client.PlayerInfo
            {
                UserName = "parentUsername"
            });

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(
            Arg.Is(playerCode),
            Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new KeyValuePair<string, string>("RealName", ""),
                new KeyValuePair<string, string>("DefaultWalletId", "36a6cdef-094f-eb11-8d88-70b5e82b6b8d"),
                new KeyValuePair<string, string>("Birthdate", "2000-04-28"),
                new KeyValuePair<string, string>("Zalo", "testZalo")
            }.ToImmutableDictionary());

        _ = fakePaymentSerivceClient.GetPlayerBankInfosAsync(
            Arg.Is(playerCode),
            Arg.Any<CancellationToken>())
            .Returns(Array.Empty<PlayerBankInfo>().ToAsyncEnumerable());

        _ = fakeVipLevelServiceClient.PlayerVipLevelGetAsync(
            Arg.Is(playerCode),
            Arg.Any<CancellationToken>())
            .Returns(new Tgs.VipLevelService.Common.ViewModels.PlayerVipLevelInfoViewModel(
                ResellerId: Guid.NewGuid(),
                ResellerCode: "Demo",
                LicenseeId: Guid.NewGuid(),
                LicenseeCode: "TGS",
                BrandId: brandId,
                BrandCode: "GPI",
                PlayerCode: "4ucmqplxb029",
                Username: "4ucmqplxb029",
                VipLevelId: vipLevelId,
                VipLevel: 1,
                AutoState: Tgs.VipLevelService.Common.PlayerVipLevelAutoState.Enable,
                AccumDepositId: Guid.NewGuid(),
                AccumDeposit: 0,
                AccumTurnoverId: Guid.NewGuid(),
                AccumTurnover: 0,
                LevelUpAccumulateBetAmount: 199m,
                LevelUpAccumulateDeposit: 99m,
                FullLevel: false,
                NextVipLevelId: NextVipLevelId,
                NextVipLevel: 2));

        _ = fakePlayerBucketClient.ReadBucketsAsync(
            Arg.Is(playerCode),
            Arg.Any<CancellationToken>())
            .Returns(new ReadBucketResult(0, null));

        _ = fakePlayerServiceClient.GetPlayerStatusAsync(
            Arg.Is(playerCode),
            Arg.Any<CancellationToken>())
            .Returns(Tgs.PlayerService.Client.PlayerStatus.Enabled);

        var expected = new
        {
            Errors = Array.Empty<string>(),
            Message = "",
            Valid = true,
            Player = new
            {
                Balance = 0M,
                Banks = Array.Empty<BankModel>(),
                Birthdate = "2000/04/28",
                LastLoginIp = "",
                ParentUsename = "parentUsername",
                PlayerCode = playerCode,
                RealName = "",
                RegisterHost = "",
                RegisterIp = "",
                Status = "Enabled",
                StatusName = "Enabled",
                Username = playerCode,
                VipId = vipLevelId,
                VipLevel = 1,
                VipLevelAutoState = true,
                AccumDeposit = 0.0m,
                AccumTurnover = 0.0m,
                LevelUpAccumulateBetAmount = 199m,
                LevelUpAccumulateDeposit = 99m,
                FullLevel = false,
                NextVipLevelId,
                NextVipLevel = 2,
                WageringRequirement = 0,
                IdentityAuthentication = true,
                Zalo = "testZalo",
            },
        };

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getSinglePlayer(playerCode);
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: [playerCode]);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<GetPlayerResponse>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.True(result!.Player.IdentityAuthentication);
        Assert.Equal(playerCode, result!.Player.PlayerCode);
    }

    [Fact]
    public async Task 依據UserName前置查詢PlayerCodes()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

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
                .AddSingleton(fakePlayerServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var userNamePrefix = "prefix";

        _ = fakePlayerServiceClient.ListPlayerCodesByUserNameAsync(
            Arg.Is(brandId),
            Arg.Is(userNamePrefix),
            Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new KeyValuePair<string, string>("username1", "player1"),
                new KeyValuePair<string, string>("username2", "player2")
            }.ToAsyncEnumerable());

        var expected = new[]
        {
            new KeyValuePair<string, string>("player1", "username1"),
            new KeyValuePair<string, string>("player2", "username2")
        };

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, userNamePrefix) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.listPlayerCodesByUserNamePrefix(brandId, userNamePrefix);
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<JsonDocument[]>(
            nodeModule,
            args: [brandId, userNamePrefix]);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual.SelectMany(doc => doc.RootElement
            .EnumerateObject()
            .Select(prop => new KeyValuePair<string, string>(
                prop.Name,
                prop.Value.GetString()!))));
    }

    [Fact]
    public async Task 查詢玩家歷史紀錄()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeUserService = Substitute.For<ITgsQueries<TGS.Core.DataServices.Tgs.Models.User>>();

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
                .AddSingleton(fakeUserService))
            .Build();

        await web.StartAsync();

        var playerCode = "playerCode";
        var from = DateTime.UtcNow.AddDays(-1);
        var to = DateTime.UtcNow;

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        var conditions = new GetPlayerHistoriesQueryConditions(playerCode, from, to);

        _ = fakePlayerServiceClient.CountPlayerHistoriesAsync(conditions, Arg.Any<CancellationToken>())
            .Returns(3);

        var eventId = Guid.NewGuid();
        _ = fakePlayerServiceClient.GetPlayerHistoriesAsync(
            conditions, 3, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new PlayerHistoryInfo[]
            {
                new() {
                    Action = EventAction.Create,
                    CreatedOn = DateTimeOffset.Now.AddDays(-1),
                    EventId = eventId,
                }
            }.ToAsyncEnumerable());

        _ = fakeUserService
            .GetQueries()
            .Returns(Array.Empty<TGS.Core.DataServices.Tgs.Models.User>().AsQueryable());

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode, request, page, rows) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getPlayerHistories(playerCode, request, page, rows);
            };
        """;

        var request = new PlayerHistoriesQueryViewModel
        {
            From = from,
            To = to,
        };

        var page = 1;
        var rows = 1;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: [playerCode, request, page, rows]);

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<PagedResponse<PlayerHistoryViewModel>>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.NotNull(result);
        Assert.True(result.Valid);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(eventId, result.Items.First().EventId);
    }

    [Fact]
    public async Task 查詢玩家歷史紀錄內容()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeUserService = Substitute.For<ITgsQueries<TGS.Core.DataServices.Tgs.Models.User>>();

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
                .AddSingleton(fakeUserService))
            .Build();

        var eventId = Guid.NewGuid();
        _ = fakePlayerServiceClient.GetPlayerHistoryAsync(
            eventId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(
                new PlayerEventInfo
                {
                    Action = EventAction.Create,
                    ChangeSets = JsonDocument.Parse("{}"),
                    CreatedOn = DateTime.UtcNow.AddDays(-1),
                    Modifier = null,
                }
            );

        _ = fakeUserService
            .GetQueries()
            .Returns(Array.Empty<TGS.Core.DataServices.Tgs.Models.User>().AsQueryable());

        await web.StartAsync();

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (eventId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getPlayerHistory(eventId);
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: [eventId]);

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<PlayerHistoryContentViewModel>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.NotNull(result);
        Assert.Equal(EventAction.Create, result.Action);
    }

    [Fact]
    public async Task 修改玩家上級代理()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakeAgentClient = Substitute.For<IAgentClient>();

        var playerCode = "playerCode";
        var brandId = Guid.NewGuid();
        var agentUsername = "agentUsername";
        var agentId = Guid.NewGuid();

        _ = fakePlayerServiceClient
            .GetPlayerInfoAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Tgs.PlayerService.Client.PlayerInfo
            {
                PlayerCode = playerCode,
                BrandId = brandId,
            });
        _ = fakeAgentClient
            .GetAgentInfoByUsernameAsync(Arg.Is(new Tgs.Agent.Client.Models.GetAgentInfoByUsernameRequest(brandId, agentUsername)), Arg.Any<CancellationToken>())
            .Returns(new Tgs.Agent.Client.Models.AgentInfoModel(
                Id: agentId,
                Username: agentUsername,
                null,
                null,
                null,
                null,
                null,
                "invitationCode",
                null,
                DateTimeOffset.UtcNow.AddMonths(-1),
                Status: Tgs.Agent.Client.Models.Enums.AgentStatus.Enabled,
                Tgs.Agent.Client.Models.Enums.AgentType.Cash,
                Tgs.Agent.Client.Models.Enums.AgentLevelType.Chief,
                0m,
                0,
                0m,
                brandId,
                Guid.NewGuid(),
                null,
                false,
                true));

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
                .AddSingleton(fakeAgentClient)
                .AddSingleton(fakeTgsRepository))
            .Build();

        await web.StartAsync();

        var request = new UpdatePlayerAgentViewModel
        {
            AgentUsername = "agentUsername"
        };

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.setPlayerAgent(playerCode, request);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: [playerCode, request]);

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);
    }

    [Fact]
    public async Task 修改玩家狀態()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakeGameProvidersBackofficePluggableClient = Substitute.For<IGameProvidersBackofficePluggableClient>();

        var playerCode = "playerCode";

        _ = fakePlayerServiceClient
            .GetPlayerInfoAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Tgs.PlayerService.Client.PlayerInfo
            {
                PlayerCode = playerCode,
            });

        _ = fakePlayerServiceClient
            .GetPlayerStatusAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(Tgs.PlayerService.Client.PlayerStatus.Enabled);

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
                .AddSingleton(fakeTgsRepository)
                .AddSingleton(fakeGameProvidersBackofficePluggableClient))
            .Build();

        await web.StartAsync();

        var request = new UpdatePlayerStatusViewModel
        {
            Status = TGS.Core.Models.Enums.PlayerStatus.InBlacklist
        };

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.setPlayerStatus(playerCode, request);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: [playerCode, request]);

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);
    }

    [Fact]
    public async Task 修改玩家信息_Vip等級()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();
        var fakeSetPlayerVipLevelService = Substitute.For<ICommandService<SetPlayerVipLevelCommand>>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var playerCode = "playerCode";
        var modifyVipId = "a5e5afe0-e87b-483d-98db-71629ba93290";

        _ = fakePlayerServiceClient
            .GetPlayerInfoAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Tgs.PlayerService.Client.PlayerInfo
            {
                PlayerCode = playerCode,
                VipLevelId = Guid.Parse("3a6b6f1d-3094-4b66-3c7a-08d9262ce1a5")
            });

        _ = fakeVipLevelServiceClient
            .PlayerVipLevelGetAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Tgs.VipLevelService.Common.ViewModels.PlayerVipLevelInfoViewModel
            (
                ResellerId: Guid.NewGuid(),
                ResellerCode: "Demo",
                LicenseeId: Guid.NewGuid(),
                LicenseeCode: "TGS",
                BrandId: Guid.NewGuid(),
                BrandCode: "GPI",
                PlayerCode: playerCode,
                Username: playerCode,
                VipLevelId: Guid.Parse("3a6b6f1d-3094-4b66-3c7a-08d9262ce1a5"),
                VipLevel: 1,
                AutoState: Tgs.VipLevelService.Common.PlayerVipLevelAutoState.Enable,
                AccumDepositId: Guid.NewGuid(),
                AccumDeposit: 0,
                AccumTurnoverId: Guid.NewGuid(),
                AccumTurnover: 0,
                LevelUpAccumulateBetAmount: 1000m,
                LevelUpAccumulateDeposit: 1000m,
                FullLevel: false,
                NextVipLevelId: Guid.Parse("a5e5afe0-e87b-483d-98db-71629ba93290"),
                NextVipLevel: 2
            ));

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
                .AddSingleton(fakeVipLevelServiceClient)
                .AddSingleton(fakeSetPlayerVipLevelService)
                .AddSingleton(fakeTgsRepository))
            .Build();

        await web.StartAsync();

        var request = new SetPlayerInfoRequest
        {
            { nameof(PlayerPropertyName.VipId), modifyVipId }
        };

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.setPlayerInfo(playerCode, request);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: [playerCode, request]);

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);
    }

    [Fact]
    public async Task 修改玩家信息_Vip自動狀態()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();
        var fakeSetPlayerVipLevelService = Substitute.For<ICommandService<SetPlayerVipLevelCommand>>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var playerCode = "playerCode";
        var modifyVipLevelAutoState = "False";

        _ = fakePlayerServiceClient
            .GetPlayerInfoAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Tgs.PlayerService.Client.PlayerInfo
            {
                PlayerCode = playerCode,
                VipLevelId = Guid.Parse("3a6b6f1d-3094-4b66-3c7a-08d9262ce1a5")
            });

        _ = fakeVipLevelServiceClient
            .PlayerVipLevelGetAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Tgs.VipLevelService.Common.ViewModels.PlayerVipLevelInfoViewModel
            (
                ResellerId: Guid.NewGuid(),
                ResellerCode: "Demo",
                LicenseeId: Guid.NewGuid(),
                LicenseeCode: "TGS",
                BrandId: Guid.NewGuid(),
                BrandCode: "GPI",
                PlayerCode: playerCode,
                Username: playerCode,
                VipLevelId: Guid.Parse("3a6b6f1d-3094-4b66-3c7a-08d9262ce1a5"),
                VipLevel: 1,
                AutoState: Tgs.VipLevelService.Common.PlayerVipLevelAutoState.Enable,
                AccumDepositId: Guid.NewGuid(),
                AccumDeposit: 0,
                AccumTurnoverId: Guid.NewGuid(),
                AccumTurnover: 0,
                LevelUpAccumulateBetAmount: 1000m,
                LevelUpAccumulateDeposit: 1000m,
                FullLevel: false,
                NextVipLevelId: Guid.Parse("a5e5afe0-e87b-483d-98db-71629ba93290"),
                NextVipLevel: 2
            ));

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
                .AddSingleton(fakeVipLevelServiceClient)
                .AddSingleton(fakeSetPlayerVipLevelService)
                .AddSingleton(fakeTgsRepository))
            .Build();

        await web.StartAsync();

        var request = new SetPlayerInfoRequest
        {
            { nameof(PlayerPropertyName.VipLevelAutoState), modifyVipLevelAutoState }
        };

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.setPlayerInfo(playerCode, request);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: [playerCode, request]);

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);
    }

    [Fact]
    public async Task 修改玩家信息__Vip等級_Vip自動狀態()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();
        var fakeSetPlayerVipLevelService = Substitute.For<ICommandService<SetPlayerVipLevelCommand>>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var playerCode = "playerCode";
        var modifyVipLevelAutoState = "False";
        var modifyVipId = "a5e5afe0-e87b-483d-98db-71629ba93290";

        _ = fakePlayerServiceClient
            .GetPlayerInfoAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Tgs.PlayerService.Client.PlayerInfo
            {
                PlayerCode = playerCode,
                VipLevelId = Guid.Parse("3a6b6f1d-3094-4b66-3c7a-08d9262ce1a5")
            });

        _ = fakeVipLevelServiceClient
            .PlayerVipLevelGetAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Tgs.VipLevelService.Common.ViewModels.PlayerVipLevelInfoViewModel
            (
                ResellerId: Guid.NewGuid(),
                ResellerCode: "Demo",
                LicenseeId: Guid.NewGuid(),
                LicenseeCode: "TGS",
                BrandId: Guid.NewGuid(),
                BrandCode: "GPI",
                PlayerCode: playerCode,
                Username: playerCode,
                VipLevelId: Guid.Parse("3a6b6f1d-3094-4b66-3c7a-08d9262ce1a5"),
                VipLevel: 1,
                AutoState: Tgs.VipLevelService.Common.PlayerVipLevelAutoState.Enable,
                AccumDepositId: Guid.NewGuid(),
                AccumDeposit: 0,
                AccumTurnoverId: Guid.NewGuid(),
                AccumTurnover: 0,
                LevelUpAccumulateBetAmount: 1000m,
                LevelUpAccumulateDeposit: 1000m,
                FullLevel: false,
                NextVipLevelId: Guid.Parse("a5e5afe0-e87b-483d-98db-71629ba93290"),
                NextVipLevel: 2
            ));

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
                .AddSingleton(fakeVipLevelServiceClient)
                .AddSingleton(fakeSetPlayerVipLevelService)
                .AddSingleton(fakeTgsRepository))
            .Build();

        await web.StartAsync();

        var request = new SetPlayerInfoRequest
        {
            { nameof(PlayerPropertyName.VipId), modifyVipId },
            { nameof(PlayerPropertyName.VipLevelAutoState), modifyVipLevelAutoState }
        };

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.setPlayerInfo(playerCode, request);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: [playerCode, request]);

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);
    }

    [Fact]
    public async Task 修改玩家密碼()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var playerCode = "playereCode";
        var password = "newPassword";
        _ = fakePlayerServiceClient
            .GetPlayerInfoAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Tgs.PlayerService.Client.PlayerInfo
            {
                PlayerCode = playerCode,
            });

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
                .AddSingleton(fakeTgsRepository))
            .Build();

        await web.StartAsync();

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode, password) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.setPlayerPassword(playerCode, password);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: [playerCode, password]);

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);
    }

    [Fact]
    public async Task 修改玩家聯絡資訊_Email()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var playerCode = "playerCode";
        var brandId = Guid.NewGuid();
        var modifyEmail = "test@test.com";

        _ = fakePlayerServiceClient
            .GetPlayerInfoAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Tgs.PlayerService.Client.PlayerInfo
            {
                BrandId = brandId,
                PlayerCode = playerCode,
            });

        _ = fakePlayerServiceClient
            .GetPlayerPropertiesAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string> { { "Email", "old@test.com" } }.ToImmutableDictionary());

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
                .AddSingleton(fakeTgsRepository))
            .Build();

        await web.StartAsync();

        var request = new SetPlayerContactRequest
        {
            { PlayerPropertyName.Email.ToString(), modifyEmail }
        };

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.setPlayerContact(playerCode, request);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: [playerCode, request]);

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);
    }

    [Fact]
    public async Task 修改玩家聯絡資訊_電話號碼()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var playerCode = "playereCode";

        _ = fakePlayerServiceClient
            .GetPlayerInfoAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Tgs.PlayerService.Client.PlayerInfo
            {
                PlayerCode = playerCode,
            });

        _ = fakePlayerServiceClient
            .GetPlayerPropertiesAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string> { { "Email", "old@test.com" } }.ToImmutableDictionary());

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
                .AddSingleton(fakeTgsRepository))
            .Build();

        await web.StartAsync();

        var request = new SetPlayerContactRequest
        {
            { PlayerPropertyName.Phone.ToString(),"1223334444" }
        };

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.setPlayerContact(playerCode, request);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: [playerCode, request]);

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);
    }

    [Fact]
    public async Task 修改玩家聯絡資訊_Zalo()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var playerCode = "playereCode";

        _ = fakePlayerServiceClient
            .GetPlayerInfoAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Tgs.PlayerService.Client.PlayerInfo
            {
                PlayerCode = playerCode,
            });

        _ = fakePlayerServiceClient
            .GetPlayerPropertiesAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string> { { "Zalo", "testZalo" } }.ToImmutableDictionary());

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
                .AddSingleton(fakeTgsRepository))
            .Build();

        await web.StartAsync();

        var request = new SetPlayerContactRequest
        {
            { PlayerPropertyName.Zalo.ToString(),"testZalo" }
        };

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.setPlayerContact(playerCode, request);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: [playerCode, request]);

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);
    }
}
