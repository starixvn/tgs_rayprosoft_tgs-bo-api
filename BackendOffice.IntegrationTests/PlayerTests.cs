using System.Collections.Immutable;
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
using Tgs.CashQueryService.Client;
using Tgs.CashService.Client;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;
using Tgs.VipLevelService.Clients;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Application.Models.ReportPlayer;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.Domain;
using Xunit.Abstractions;
using static TGS.Services.BackOffice.Application.Models.Player.PlayerModel;
using BoEnums = TGS.Services.BackOffice.Domain.Enums;

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
                new VipLevelInfo(
                    vipId1,
                    1,
                    brandId,
                    0M,
                    0M,
                    0M,
                    0M,
                    false,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    -1,
                    -1M),
                new VipLevelInfo(
                    Guid.NewGuid(),
                    2,
                    brandId,
                    0M,
                    0M,
                    0M,
                    0M,
                    false,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    -1,
                    -1M)
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

        _ = fakeMediator.Send(Arg.Any<UpdatePlayerRequest>(), Arg.Any<CancellationToken>())
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
        var actual = await StaticNodeJSService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { playerCode, birthdate.ToString("yyyy/MM/dd") });

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
                new VipLevelInfo(
                    vipId1,
                    1,
                    brandId,
                    0M,
                    0M,
                    0M,
                    0M,
                    false,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    -1,
                    -1M),
                new VipLevelInfo(
                    Guid.NewGuid(),
                    2,
                    brandId,
                    0M,
                    0M,
                    0M,
                    0M,
                    false,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    -1,
                    -1M)
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

    [Fact]
    public async Task 依據PlayerCode查詢Player資料()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePaymentSerivceClient = Substitute.For<IPaymentServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakeVipLevelsQueryService = Substitute.For<IQueryService<AllVipLevelsQuery, VipLevelInfo>>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

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
                .AddSingleton(fakeVipLevelsQueryService)
                .AddSingleton(fakeCashServiceClient))
            .Build();

        await web.StartAsync();

        var playerCode = "4ucmqplxb029";
        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var vipLevelId = Guid.Parse("bcd5a809-8b90-ec11-a746-ac82478e2af1");

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
                new KeyValuePair<string, string>("Birthdate", "2000-04-28")
            }.ToImmutableDictionary());

        _ = fakePaymentSerivceClient.GetPlayerBankInfosAsync(
            Arg.Is(playerCode),
            Arg.Any<CancellationToken>())
            .Returns(Array.Empty<PlayerBankInfo>().ToAsyncEnumerable());

        _ = fakeVipLevelsQueryService.QueryAsync(
            Arg.Is(new AllVipLevelsQuery(brandId)),
            Arg.Any<CancellationToken>())
            .Returns(new VipLevelInfo[]
            {
                new VipLevelInfo(
                    vipLevelId,
                    1,
                    brandId,
                    0M,
                    0M,
                    0M,
                    0M,
                    true,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    -1,
                    -1M)
            }.ToAsyncEnumerable());

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
                VipId = "bcd5a809-8b90-ec11-a746-ac82478e2af1",
                VipLevel = 1,
                WageringRequirement = 0,
                IdentityAuthentication = true,
            },
        };

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
        var actual = await StaticNodeJSService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { playerCode });

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
        var actual = await StaticNodeJSService.InvokeFromStringAsync<JsonDocument[]>(
            nodeModule,
            args: new object[] { brandId, userNamePrefix });

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual.SelectMany(doc => doc.RootElement
            .EnumerateObject()
            .Select(prop => new KeyValuePair<string, string>(
                prop.Name,
                prop.Value.GetString()!))));
    }

    [Fact]
    public async Task 查詢玩家帳變紀錄()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakeCashQueryClient = Substitute.For<ICashQueryClient>();

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
                .AddSingleton(fakeCashServiceClient)
                .AddSingleton(fakeCashQueryClient))
            .Build();

        await web.StartAsync();

        var playerCode = "playerCode";
        var defaultWalletId = Guid.Parse("b97b0c64-8e66-47f8-0998-08d9b932b074");
        var from = DateTime.UtcNow.AddDays(-1);
        var to = DateTime.UtcNow;

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var page = 1;
        var rows = 20;
        var sortby = "recordedOn";
        var orderby = SortOrder.Ascending;
        var showAll = false;

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(
            Arg.Is(playerCode),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                { "DefaultWalletId", defaultWalletId.ToString() }
            }.ToImmutableDictionary());

        var transactionId1 = Guid.Parse("ce8a455e-7f01-0000-0001-0000f79e2257");
        var transacationId2 = Guid.Parse("cf29d2e7-7f01-0000-0001-0000f79e2257");

        var transactionLogs = new Tgs.CashService.Client.PagingResponse<TransactionLog>(
            Items: new List<TransactionLog>{
                new TransactionLog(transactionId1,DateTime.UtcNow,0,100M, 200M,0M,0M, null,null),
                new TransactionLog(transacationId2,DateTime.UtcNow,0,100M, 200M,0M,0M, null,null),
            },
            Page: page,
            TotalCount: 2
        );

        var queryTransTypes = Enum
            .GetValues(typeof(BoEnums.TransactionType))
            .Cast<BoEnums.TransactionType>()
            .Where(x => x is not BoEnums.TransactionType.WithdrawCommit
                and not BoEnums.TransactionType.TransferCommit)
            .Select(x => (int)x)
            .ToArray();

        _ = fakeCashServiceClient
            .QueryTransactionLogsAsync(
                Arg.Is(defaultWalletId),
                Arg.Is(from),
                Arg.Is(to),
                Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(queryTransTypes)),
                Arg.Any<PagingConditions>(),
                Arg.Any<CancellationToken>())
            .Returns(transactionLogs);

        var transactionIds = transactionLogs.Items.Select(x => x.Id);

        _ = fakeCashQueryClient.QueryPlayerAdjustIncludeInNetProfitRecordsAsync(
                Arg.Any<IEnumerable<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns(new PlayerAdjustRecord[] {
                new PlayerAdjustRecord(transactionId1,true),
                new PlayerAdjustRecord(transacationId2,false)
            }.ToAsyncEnumerable());

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getAccountChanges(request);
            };
        """;

        var request = new GetPlayerAccountChangesReportRequest
        {
            AccountChangeType = null,
            From = from,
            To = to,
            PlayerCode = playerCode,
            BrandId = brandId,
            OrderBy = orderby,
            Page = page,
            Rows = rows,
            SortBy = sortby,
            ShowAll = showAll
        };

        var expected = new PagedResponse<PlayerAccountChangeReportModel>()
        {
            TotalCount = 2,
            Valid = true,
            Items = new[] {
                new PlayerAccountChangeReportModel{
                    TransactionId = transactionId1,
                    IncludeInNetProfit = true,
                    Amount = 100M,
                    Balance = 200M,
                    GameActionId = null,
                    GameActionType = null,
                    RecordedOn = DateTime.UtcNow,
                    Remark = "",
                    TransactionType = TGS.Services.BackOffice.Domain.Enums.TransactionType.Deposit,
                    ExternalTransactionId = ""
                },
                new PlayerAccountChangeReportModel{
                    TransactionId = transacationId2,
                    IncludeInNetProfit = false,
                    Amount = 100M,
                    Balance = 200M,
                    GameActionId = null,
                    GameActionType = null,
                    RecordedOn = DateTime.UtcNow,
                    Remark = "",
                    TransactionType = TGS.Services.BackOffice.Domain.Enums.TransactionType.Deposit,
                    ExternalTransactionId = ""
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
        var result = JsonSerializer.Deserialize<PagedResponse<PlayerAccountChangeReportModel>>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.True(result?.Valid);
        Assert.Equal(2, result?.TotalCount);
    }

    [Fact]
    public async Task 查詢玩家帳變紀錄總金額()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

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
                .AddSingleton(fakeCashServiceClient))
            .Build();

        await web.StartAsync();

        var playerCode = "playerCode";
        var defaultWalletId = Guid.Parse("b97b0c64-8e66-47f8-0998-08d9b932b074");
        var from = DateTime.UtcNow.AddDays(-1);
        var to = DateTime.UtcNow;

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(
            Arg.Is(playerCode),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                { "DefaultWalletId", defaultWalletId.ToString() }
            }.ToImmutableDictionary());

        var queryTransTypes = Enum
            .GetValues(typeof(BoEnums.TransactionType))
            .Cast<BoEnums.TransactionType>()
            .Where(x => x is not BoEnums.TransactionType.WithdrawCommit
                and not BoEnums.TransactionType.TransferCommit)
            .Select(x => (int)x)
            .ToArray();

        _ = fakeCashServiceClient
            .QueryTransactionTotalAmountAsync(
                Arg.Is(defaultWalletId),
                Arg.Is(from),
                Arg.Is(to),
                Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(queryTransTypes)),
                Arg.Any<CancellationToken>())
            .Returns(168000000M);

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getAccountChangesTotalAmount(request);
            };
        """;

        var request = new GetPlayerAccountChangesReportTotalAmountRequest
        {
            AccountChangeType = null,
            From = from,
            To = to,
            PlayerCode = playerCode,
            BrandId = brandId,
        };

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { request });

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<PlayerAccountChangesTotalModel>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.Equal(168000000M, result!.Amount);
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
                new PlayerHistoryInfo
                {
                    Action = EventAction.Create,
                    CreatedOn = DateTimeOffset.Now.AddDays(-1),
                    EventId = eventId,
                }
            }.ToAsyncEnumerable());

        _ = fakeUserService
            .GetQueries()
            .Returns(Array.Empty<TGS.Core.DataServices.Tgs.Models.User>().AsQueryable());

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
        var actual = await StaticNodeJSService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { playerCode, request, page, rows });

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
        var actual = await StaticNodeJSService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { eventId });

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
            .GetPlayerInfoAsync(Arg.Is(playerCode))
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
        var actual = await StaticNodeJSService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { playerCode, password });

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
            .GetPlayerInfoAsync(Arg.Is(playerCode))
            .Returns(new Tgs.PlayerService.Client.PlayerInfo
            {
                BrandId = brandId,
                PlayerCode = playerCode,
            });

        _ = fakePlayerServiceClient
            .GetPlayerPropertiesAsync(Arg.Is(playerCode))
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
        var actual = await StaticNodeJSService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { playerCode, request });

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

        var playerCode = "playereCode";
        var request = new SetPlayerContactRequest
        {
            { PlayerPropertyName.Phone.ToString(),"1223334444" }
        };

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
        var actual = await StaticNodeJSService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { playerCode, request });

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);
    }
}
