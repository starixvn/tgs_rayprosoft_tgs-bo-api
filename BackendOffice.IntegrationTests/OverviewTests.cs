using System.Runtime.InteropServices;
using ExpectedObjects;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.BrandCertificate.Clients;
using Tgs.GameWagers.Client;
using Tgs.GameWagers.Common;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.BackOffice.Domain.Enums;
using TGS.Services.BackOffice.Infrastructure.DataService;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class OverviewTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public OverviewTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 查詢十大玩家_存款()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeGameWagersClient = Substitute.For<IGameWagersClient>();
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
                .AddSingleton(fakePaymentServiceClient)
                .AddSingleton(fakeBrandServiceClient)
                .AddSingleton(fakeGameWagersClient)
                .AddSingleton(fakePlayerServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var sortType = TopTenPlayerSortType.Deposit;
        var ascending = false;

        var brandCode = "GPI";
        var playerCodes = new string[] { "4uel5ke0uf3k" };

        _ = fakeBrandServiceClient
            .GetBrandInfoAsync(
                Arg.Is(brandId),
                Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Code = brandCode
            });

        _ = fakePaymentServiceClient
            .GetCurrentDayTopTenDepositPlayersAsync(
                Arg.Is(brandId),
                Arg.Any<CancellationToken>())
            .Returns(new TopTenDepositWithdrawInfo[]
            {
                new TopTenDepositWithdrawInfo
                {
                    PlayerCode = playerCodes[0],
                    DepositTotalAmount = 16888m,
                    WithdrawTotalAmount = 88m
                }
            }.ToAsyncEnumerable());

        _ = fakeGameWagersClient
            .BrandWagerPlayerStatsPlayerQueryAsync(
                Arg.Is(brandCode),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Is<IEnumerable<string>>(x => x.SequenceEqual(playerCodes)),
                Arg.Any<CancellationToken>())
            .Returns(new BrandWagerPlayerStatsInfo[]
            {
                new BrandWagerPlayerStatsInfo(
                    GameProviderId: Guid.NewGuid(),
                    GameProviderCode: "GPCode",
                    ResellerId: Guid.NewGuid(),
                    ResellerCode: "ResellerCode",
                    LicenseeId: Guid.NewGuid(),
                    LicenseeCode: "LicenseeCode",
                    brandId,
                    brandCode,
                    playerCodes[0],
                    "username",
                    CurrencyCode: "CNY",
                    Count: 1,
                    Bet: 100m,
                    ValidBet: 100m,
                    Settle: 0m,
                    Commission: 0m,
                    Turnover: 100m,
                    WinLoss: -100m)
            }.ToAsyncEnumerable());

        _ = fakePlayerServiceClient
            .QueryPlayerInfoListAsync(
                Arg.Is(brandId),
                Arg.Is<IEnumerable<string>>(p => p.SequenceEqual(playerCodes)),
                Arg.Any<CancellationToken>())
            .Returns(new Tgs.PlayerService.Client.PlayerInfo[]
            {
                new Tgs.PlayerService.Client.PlayerInfo
                {
                    PlayerCode = playerCodes[0],
                    UserName = "username"
                }
            }.ToAsyncEnumerable());

        var expected = new TopTenPlayerViewModel[]
        {
            new TopTenPlayerViewModel
            {
                PlayerCode = playerCodes[0],
                PlayerUsername = "username",
                Deposit = 16888m,
                Withdraw = 88m,
                Bet = 100,
                Settle = 0m,
                WinLoss = -100m
            }
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, sortType, ascending) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getTopTenPlayers(brandId, sortType, ascending);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<TopTenPlayerViewModel[]>(
            nodeModule,
            args: new object[] { brandId, sortType, ascending });

        // Assert
        expected.ToExpectedObject().ShouldEqual(actual);
    }

    [Fact]
    public async Task 查詢十大玩家_投注()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeGameWagersClient = Substitute.For<IGameWagersClient>();
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
                .AddSingleton(fakePaymentServiceClient)
                .AddSingleton(fakeBrandServiceClient)
                .AddSingleton(fakeGameWagersClient)
                .AddSingleton(fakePlayerServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var sortType = TopTenPlayerSortType.Bet;
        var ascending = false;

        var brandCode = "GPI";
        var playerCodes = new string[] { "4uel5ke0uf3k" };

        _ = fakeBrandServiceClient
            .GetBrandInfoAsync(
                Arg.Is(brandId),
                Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Code = brandCode
            });

        _ = fakeGameWagersClient
            .BrandWagerPlayerStatsQueryAsync(
                Arg.Is(brandCode),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Is(BrandWagerPlayerMinsStatsSort.Bet),
                Arg.Is(false),
                page: Arg.Is(1),
                pageSize: Arg.Is(10),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new BrandWagerPlayerStatsInfo[]
            {
                new BrandWagerPlayerStatsInfo(
                   GameProviderId: Guid.NewGuid(),
                   "GPCode",
                   ResellerId: Guid.NewGuid(),
                   "ResellerCode",
                   LicenseeId: Guid.NewGuid(),
                   "LicenseeCode",
                   brandId,
                   brandCode,
                   playerCodes[0],
                   "username",
                   CurrencyCode: "CNY",
                   Count: 1,
                   Bet:100m,
                   ValidBet: 100m,
                   Settle: 0m,
                   Commission: 0m,
                   Turnover: 100m,
                   WinLoss: -100m)
            }.ToAsyncEnumerable());

        _ = fakePaymentServiceClient
            .GetCurrentDayDepositWithdrawTotalAmountAsync(
                Arg.Is<IEnumerable<string>>(p => p.SequenceEqual(playerCodes)),
                Arg.Any<CancellationToken>())
            .Returns(new TopTenDepositWithdrawInfo[]
            {
                new TopTenDepositWithdrawInfo
                {
                    PlayerCode = playerCodes[0],
                    DepositTotalAmount = 16888m,
                    WithdrawTotalAmount = 88m
                }
            }.ToAsyncEnumerable());

        _ = fakePlayerServiceClient
            .QueryPlayerInfoListAsync(
                Arg.Is(brandId),
                Arg.Is<IEnumerable<string>>(p => p.SequenceEqual(playerCodes)),
                Arg.Any<CancellationToken>())
            .Returns(new Tgs.PlayerService.Client.PlayerInfo[]
            {
                new Tgs.PlayerService.Client.PlayerInfo
                {
                    PlayerCode = playerCodes[0],
                    UserName = "username"
                }
            }.ToAsyncEnumerable());

        var expected = new TopTenPlayerViewModel[]
        {
            new TopTenPlayerViewModel
            {
                PlayerCode = playerCodes[0],
                PlayerUsername = "username",
                Deposit = 16888m,
                Withdraw = 88m,
                Bet = 100,
                Settle = 0m,
                WinLoss = -100m
            }
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, sortType, ascending) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getTopTenPlayers(brandId, sortType, ascending);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<TopTenPlayerViewModel[]>(
            nodeModule,
            args: new object[] { brandId, sortType, ascending });

        // Assert
        expected.ToExpectedObject().ShouldEqual(actual);
    }

    [Fact]
    public async Task 查詢十大玩家_派彩()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeGameWagersClient = Substitute.For<IGameWagersClient>();
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
                .AddSingleton(fakePaymentServiceClient)
                .AddSingleton(fakeBrandServiceClient)
                .AddSingleton(fakeGameWagersClient)
                .AddSingleton(fakePlayerServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var sortType = TopTenPlayerSortType.Settle;
        var ascending = false;

        var brandCode = "GPI";
        var playerCodes = new string[] { "4uel5ke0uf3k" };

        _ = fakeBrandServiceClient
            .GetBrandInfoAsync(
                Arg.Is(brandId),
                Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Code = brandCode
            });

        _ = fakeGameWagersClient
            .BrandWagerPlayerStatsQueryAsync(
                Arg.Is(brandCode),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Is(BrandWagerPlayerMinsStatsSort.Settle),
                Arg.Is(false),
                page: Arg.Is(1),
                pageSize: Arg.Is(10),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new BrandWagerPlayerStatsInfo[]
            {
                new BrandWagerPlayerStatsInfo(Guid.NewGuid(),"GPCode", Guid.NewGuid(),"ResellerCode",Guid.NewGuid(),"LicenseeCode",brandId,brandCode,playerCodes[0],"username","CNY",1,100m,100m,0m,0m,100m,-100m)
            }.ToAsyncEnumerable());

        _ = fakePaymentServiceClient
            .GetCurrentDayDepositWithdrawTotalAmountAsync(
                Arg.Is<IEnumerable<string>>(p => p.SequenceEqual(playerCodes)),
                Arg.Any<CancellationToken>())
            .Returns(new TopTenDepositWithdrawInfo[]
            {
                new TopTenDepositWithdrawInfo
                {
                    PlayerCode = playerCodes[0],
                    DepositTotalAmount = 16888m,
                    WithdrawTotalAmount = 88m
                }
            }.ToAsyncEnumerable());

        _ = fakePlayerServiceClient
            .QueryPlayerInfoListAsync(
                Arg.Is(brandId),
                Arg.Is<IEnumerable<string>>(p => p.SequenceEqual(playerCodes)),
                Arg.Any<CancellationToken>())
            .Returns(new Tgs.PlayerService.Client.PlayerInfo[]
            {
                new Tgs.PlayerService.Client.PlayerInfo
                {
                    PlayerCode = playerCodes[0],
                    UserName = "username"
                }
            }.ToAsyncEnumerable());

        var expected = new TopTenPlayerViewModel[]
        {
            new TopTenPlayerViewModel
            {
                PlayerCode = playerCodes[0],
                PlayerUsername = "username",
                Deposit = 16888m,
                Withdraw = 88m,
                Bet = 100,
                Settle = 0m,
                WinLoss = -100m
            }
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, sortType, ascending) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getTopTenPlayers(brandId, sortType, ascending);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<TopTenPlayerViewModel[]>(
            nodeModule,
            args: new object[] { brandId, sortType, ascending });

        // Assert
        expected.ToExpectedObject().ShouldEqual(actual);
    }

    [Fact]
    public async Task 查詢十大玩家_提款()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeGameWagersClient = Substitute.For<IGameWagersClient>();
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
                .AddSingleton(fakePaymentServiceClient)
                .AddSingleton(fakeBrandServiceClient)
                .AddSingleton(fakeGameWagersClient)
                .AddSingleton(fakePlayerServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var sortType = TopTenPlayerSortType.Withdraw;
        var ascending = false;

        var brandCode = "GPI";
        var playerCodes = new string[] { "4uel5ke0uf3k" };

        _ = fakeBrandServiceClient
            .GetBrandInfoAsync(
                Arg.Is(brandId),
                Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Code = brandCode
            });

        _ = fakePaymentServiceClient
            .GetCurrentDayTopTenWithdrawPlayersAsync(
                Arg.Is(brandId),
                Arg.Any<CancellationToken>())
            .Returns(new TopTenDepositWithdrawInfo[]
            {
                new TopTenDepositWithdrawInfo
                {
                    PlayerCode = playerCodes[0],
                    DepositTotalAmount = 16888m,
                    WithdrawTotalAmount = 88m
                }
            }.ToAsyncEnumerable());

        _ = fakeGameWagersClient
            .BrandWagerPlayerStatsPlayerQueryAsync(
                Arg.Is(brandCode),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Is<IEnumerable<string>>(x => x.SequenceEqual(playerCodes)),
                Arg.Any<CancellationToken>())
            .Returns(new BrandWagerPlayerStatsInfo[]
            {
                new BrandWagerPlayerStatsInfo(Guid.NewGuid(),"GPCode", Guid.NewGuid(),"ResellerCode",Guid.NewGuid(),"LicenseeCode",brandId,brandCode,playerCodes[0],"username","CNY",1,100m,100m,0m,0m,100m,-100m)
            }.ToAsyncEnumerable());

        _ = fakePlayerServiceClient
            .QueryPlayerInfoListAsync(
                Arg.Is(brandId),
                Arg.Is<IEnumerable<string>>(p => p.SequenceEqual(playerCodes)),
                Arg.Any<CancellationToken>())
            .Returns(new Tgs.PlayerService.Client.PlayerInfo[]
            {
                new Tgs.PlayerService.Client.PlayerInfo
                {
                    PlayerCode = playerCodes[0],
                    UserName = "username"
                }
            }.ToAsyncEnumerable());

        var expected = new TopTenPlayerViewModel[]
        {
            new TopTenPlayerViewModel
            {
                PlayerCode = playerCodes[0],
                PlayerUsername = "username",
                Deposit = 16888m,
                Withdraw = 88m,
                Bet = 100,
                Settle = 0m,
                WinLoss = -100m
            }
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, sortType, ascending) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getTopTenPlayers(brandId, sortType, ascending);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<TopTenPlayerViewModel[]>(
            nodeModule,
            args: new object[] { brandId, sortType, ascending });

        // Assert
        expected.ToExpectedObject().ShouldEqual(actual);
    }

    [Fact]
    public async Task 查詢十大玩家_輸贏升冪()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeGameWagersClient = Substitute.For<IGameWagersClient>();
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
                .AddSingleton(fakePaymentServiceClient)
                .AddSingleton(fakeBrandServiceClient)
                .AddSingleton(fakeGameWagersClient)
                .AddSingleton(fakePlayerServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var sortType = TopTenPlayerSortType.WinLoss;
        var ascending = true;

        var brandCode = "GPI";
        var playerCodes = new string[] { "4uel5ke0uf3k" };

        _ = fakeBrandServiceClient
            .GetBrandInfoAsync(
                Arg.Is(brandId),
                Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Code = brandCode
            });

        _ = fakeGameWagersClient
            .BrandWagerPlayerStatsQueryAsync(
                Arg.Is(brandCode),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Is(BrandWagerPlayerMinsStatsSort.WinLoss),
                Arg.Is(true),
                page: Arg.Is(1),
                pageSize: Arg.Is(10),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new BrandWagerPlayerStatsInfo[]
            {
                new BrandWagerPlayerStatsInfo(Guid.NewGuid(),"GPCode", Guid.NewGuid(),"ResellerCode",Guid.NewGuid(),"LicenseeCode",brandId,brandCode,playerCodes[0],"username","CNY",1,100m,100m,0m,0m,100m,-100m)
            }.ToAsyncEnumerable());

        _ = fakePaymentServiceClient
            .GetCurrentDayDepositWithdrawTotalAmountAsync(
                Arg.Is<IEnumerable<string>>(p => p.SequenceEqual(playerCodes)),
                Arg.Any<CancellationToken>())
            .Returns(new TopTenDepositWithdrawInfo[]
            {
                new TopTenDepositWithdrawInfo
                {
                    PlayerCode = playerCodes[0],
                    DepositTotalAmount = 16888m,
                    WithdrawTotalAmount = 88m
                }
            }.ToAsyncEnumerable());

        _ = fakePlayerServiceClient
            .QueryPlayerInfoListAsync(
                Arg.Is(brandId),
                Arg.Is<IEnumerable<string>>(p => p.SequenceEqual(playerCodes)),
                Arg.Any<CancellationToken>())
            .Returns(new Tgs.PlayerService.Client.PlayerInfo[]
            {
                new Tgs.PlayerService.Client.PlayerInfo
                {
                    PlayerCode = playerCodes[0],
                    UserName = "username"
                }
            }.ToAsyncEnumerable());

        var expected = new TopTenPlayerViewModel[]
        {
            new TopTenPlayerViewModel
            {
                PlayerCode = playerCodes[0],
                PlayerUsername = "username",
                Deposit = 16888m,
                Withdraw = 88m,
                Bet = 100,
                Settle = 0m,
                WinLoss = -100m
            }
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, sortType, ascending) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getTopTenPlayers(brandId, sortType, ascending);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<TopTenPlayerViewModel[]>(
            nodeModule,
            args: new object[] { brandId, sortType, ascending });

        // Assert
        expected.ToExpectedObject().ShouldEqual(actual);
    }

    [Fact]
    public async Task 查詢十大玩家_輸贏降冪()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeGameWagersClient = Substitute.For<IGameWagersClient>();
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
                .AddSingleton(fakePaymentServiceClient)
                .AddSingleton(fakeBrandServiceClient)
                .AddSingleton(fakeGameWagersClient)
                .AddSingleton(fakePlayerServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var sortType = TopTenPlayerSortType.WinLoss;
        var ascending = false;

        var brandCode = "GPI";
        var playerCodes = new string[] { "4uel5ke0uf3k" };

        _ = fakeBrandServiceClient
            .GetBrandInfoAsync(
                Arg.Is(brandId),
                Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Code = brandCode
            });

        _ = fakeGameWagersClient
            .BrandWagerPlayerStatsQueryAsync(
                Arg.Is(brandCode),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Is(BrandWagerPlayerMinsStatsSort.WinLoss),
                Arg.Is(false),
                page: Arg.Is(1),
                pageSize: Arg.Is(10),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new BrandWagerPlayerStatsInfo[]
            {
                new BrandWagerPlayerStatsInfo(Guid.NewGuid(),"GPCode",Guid.NewGuid(),"ResellerCode",Guid.NewGuid(),"LicenseeCode",brandId,brandCode,playerCodes[0],"username","CNY",1,100m,100m,0m,0m,100m,-100m)
            }.ToAsyncEnumerable());

        _ = fakePaymentServiceClient
            .GetCurrentDayDepositWithdrawTotalAmountAsync(
                Arg.Is<IEnumerable<string>>(p => p.SequenceEqual(playerCodes)),
                Arg.Any<CancellationToken>())
            .Returns(new TopTenDepositWithdrawInfo[]
            {
                new TopTenDepositWithdrawInfo
                {
                    PlayerCode = playerCodes[0],
                    DepositTotalAmount = 16888m,
                    WithdrawTotalAmount = 88m
                }
            }.ToAsyncEnumerable());

        _ = fakePlayerServiceClient
            .QueryPlayerInfoListAsync(
                Arg.Is(brandId),
                Arg.Is<IEnumerable<string>>(p => p.SequenceEqual(playerCodes)),
                Arg.Any<CancellationToken>())
            .Returns(new Tgs.PlayerService.Client.PlayerInfo[]
            {
                new Tgs.PlayerService.Client.PlayerInfo
                {
                    PlayerCode = playerCodes[0],
                    UserName = "username"
                }
            }.ToAsyncEnumerable());

        var expected = new TopTenPlayerViewModel[]
        {
            new TopTenPlayerViewModel
            {
                PlayerCode = playerCodes[0],
                PlayerUsername = "username",
                Deposit = 16888m,
                Withdraw = 88m,
                Bet = 100,
                Settle = 0m,
                WinLoss = -100m
            }
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, sortType, ascending) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getTopTenPlayers(brandId, sortType, ascending);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<TopTenPlayerViewModel[]>(
            nodeModule,
            args: new object[] { brandId, sortType, ascending });

        // Assert
        expected.ToExpectedObject().ShouldEqual(actual);
    }

    [Fact]
    public async Task 查詢十大遊戲_投注降冪()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeGameWagersClient = Substitute.For<IGameWagersClient>();
        var fakeBrandGameService = Substitute.For<IBrandGameService>();

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
                .AddSingleton(fakeGameWagersClient)
                .AddSingleton(fakeBrandGameService))
            .Build();

        await web.StartAsync();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var brandCode = "GPI";
        var sortType = TopTenGameSortType.Bet;
        var gameName = "Happy Red Packets";
        var gameProviderName = "KY";
        var winLossAscending = false;

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
                Arg.Is(brandId),
                Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                Code = brandCode,
            });

        _ = fakeGameWagersClient
            .BrandWagerGameStatsQueryAsync(
                Arg.Is(brandCode),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Is(BrandWagerGameMinsStatsSort.Bet),
                Arg.Is(false),
                page: Arg.Is(1),
                pageSize: Arg.Is(10),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new BrandWagerGameStatsInfo[]
            {
                new BrandWagerGameStatsInfo(
                    GameProviderId: Guid.Parse("a0f49ce0-2c1d-4f50-9fc5-58b610a30bf5"),
                    GameProviderCode: "KY",
                    GameId: 1000010,
                    Guid.NewGuid(),
                    "ResellerCode",
                    Guid.NewGuid(),
                    "LicenseeCode",
                    brandId,
                    brandCode,
                    "CNY",
                    1,
                    100m,
                    100m,
                    0m,
                    0m,
                    100m,
                    -100m)
            }.ToAsyncEnumerable());

        _ = fakeBrandGameService.GetBrandGameQueries()
            .Returns(new BrandGame[]
            {
                new BrandGame
                {
                    BrandId = brandId,
                    GameId = 1000010,
                    Game = new Game()
                    {
                        GameProviderId = Guid.Parse("a0f49ce0-2c1d-4f50-9fc5-58b610a30bf5"),
                        Name = gameName,
                        SubGameProvider = new SubGameProvider
                        {
                            Name = gameProviderName
                        }
                    }
                }
            }.AsQueryable());

        var expected = new TopTenGameViewModel[]
        {
            new TopTenGameViewModel
            {
                GameName = gameName,
                GameProviderName = gameProviderName,
                Bet = 100m,
                WinLoss = -100m
            }
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, sortType, winLossAscending) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getTopTenGames(brandId, sortType, winLossAscending);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<TopTenGameViewModel[]>(
            nodeModule,
            args: new object[] { brandId, sortType, winLossAscending });

        // Assert
        expected.ToExpectedObject().ShouldEqual(actual);
    }

    [Fact]
    public async Task 查詢十大遊戲_輸贏升冪()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeGameWagersClient = Substitute.For<IGameWagersClient>();
        var fakeBrandGameService = Substitute.For<IBrandGameService>();

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
                .AddSingleton(fakeGameWagersClient)
                .AddSingleton(fakeBrandGameService))
            .Build();

        await web.StartAsync();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var brandCode = "GPI";
        var sortType = TopTenGameSortType.WinLoss;
        var gameName = "Happy Red Packets";
        var gameProviderName = "KY";
        var ascending = true;

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
                Arg.Is(brandId),
                Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                Code = brandCode,
            });

        _ = fakeGameWagersClient
            .BrandWagerGameStatsQueryAsync(
                Arg.Is(brandCode),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Is(BrandWagerGameMinsStatsSort.WinLoss),
                Arg.Is(true),
                page: Arg.Is(1),
                pageSize: Arg.Is(10),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new BrandWagerGameStatsInfo[]
            {
                new BrandWagerGameStatsInfo(
                    GameProviderId: Guid.Parse("a0f49ce0-2c1d-4f50-9fc5-58b610a30bf5"),
                    GameProviderCode: "KY",
                    GameId: 1000010,
                    Guid.NewGuid(),
                    "ResellerCode",
                    Guid.NewGuid(),
                    "LicenseeCode",
                    brandId,
                    brandCode,
                    "CNY",
                    1,
                    100m,
                    100m,
                    0m,
                    0m,
                    100m,
                    -100m)
            }.ToAsyncEnumerable());

        _ = fakeBrandGameService.GetBrandGameQueries()
            .Returns(new BrandGame[]
            {
                new BrandGame
                {
                    BrandId = brandId,
                    GameId = 1000010,
                    Game = new Game()
                    {
                        GameProviderId = Guid.Parse("a0f49ce0-2c1d-4f50-9fc5-58b610a30bf5"),
                        Name = gameName,
                        SubGameProvider = new SubGameProvider
                        {
                            Name = gameProviderName
                        }
                    }
                }
            }.AsQueryable());

        var expected = new TopTenGameViewModel[]
        {
            new TopTenGameViewModel
            {
                GameName = gameName,
                GameProviderName = gameProviderName,
                Bet = 100m,
                WinLoss = -100m
            }
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, sortType, ascending) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getTopTenGames(brandId, sortType, ascending);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<TopTenGameViewModel[]>(
            nodeModule,
            args: new object[] { brandId, sortType, ascending });

        // Assert
        expected.ToExpectedObject().ShouldEqual(actual);
    }

    [Fact]
    public async Task 查詢十大遊戲_輸贏降冪()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeGameWagersClient = Substitute.For<IGameWagersClient>();
        var fakeBrandGameService = Substitute.For<IBrandGameService>();

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
                .AddSingleton(fakeGameWagersClient)
                .AddSingleton(fakeBrandGameService))
            .Build();

        await web.StartAsync();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var brandCode = "GPI";
        var sortType = TopTenGameSortType.WinLoss;
        var gameName = "Happy Red Packets";
        var gameProviderName = "KY";
        var winLossAscending = false;

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
                Arg.Is(brandId),
                Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                Code = brandCode,
            });

        _ = fakeGameWagersClient
            .BrandWagerGameStatsQueryAsync(
                Arg.Is(brandCode),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Is(BrandWagerGameMinsStatsSort.WinLoss),
                Arg.Is(false),
                page: Arg.Is(1),
                pageSize: Arg.Is(10),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new BrandWagerGameStatsInfo[]
            {
                new BrandWagerGameStatsInfo(
                    GameProviderId: Guid.Parse("a0f49ce0-2c1d-4f50-9fc5-58b610a30bf5"),
                    GameProviderCode: "KY",
                    GameId: 1000010,
                    Guid.NewGuid(),
                    "ResellerCode",
                    Guid.NewGuid(),
                    "LicenseeCode",
                    brandId,
                    brandCode,
                    "CNY",
                    1,
                    100m,
                    100m,
                    0m,
                    0m,
                    100m,
                    -100m)
            }.ToAsyncEnumerable());

        _ = fakeBrandGameService.GetBrandGameQueries()
            .Returns(new BrandGame[]
            {
                new BrandGame
                {
                    BrandId = brandId,
                    GameId = 1000010,
                    Game = new Game()
                    {
                        GameProviderId = Guid.Parse("a0f49ce0-2c1d-4f50-9fc5-58b610a30bf5"),
                        Name = gameName,
                        SubGameProvider = new SubGameProvider
                        {
                            Name = gameProviderName
                        }
                    }
                }
            }.AsQueryable());

        var expected = new TopTenGameViewModel[]
        {
            new TopTenGameViewModel
            {
                GameName = gameName,
                GameProviderName = gameProviderName,
                Bet = 100m,
                WinLoss = -100m
            }
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, sortType, winLossAscending) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getTopTenGames(brandId, sortType, winLossAscending);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<TopTenGameViewModel[]>(
            nodeModule,
            args: new object[] { brandId, sortType, winLossAscending });

        // Assert
        expected.ToExpectedObject().ShouldEqual(actual);
    }

    [Fact]
    public async Task 查詢存提差()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

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
                .AddSingleton(fakePaymentServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();

        _ = fakePaymentServiceClient
            .GetBrandCurrentDayNetDepositAsync(Arg.Is(brandId), Arg.Any<CancellationToken>())
            .Returns(168888m);

        _ = fakePaymentServiceClient
            .GetBrandReportDateNetDepositAsync(Arg.Is(brandId), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(168000m);

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getDayNetDeposit(brandId);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<NetDepositViewModel>(
            nodeModule,
            args: new object[] { brandId });

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(168888m, actual.TodayNetDeposit);
        Assert.Equal(168000m, actual.YesterdayNetDeposit);
    }

    [Fact]
    public async Task 查詢存款成功率()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

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
                .AddSingleton(fakePaymentServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();

        _ = fakePaymentServiceClient
            .GetCurrentDayDepositSuccessRateAsync(
                Arg.Is(brandId),
                Arg.Any<CancellationToken>())
            .Returns(100m);

        _ = fakePaymentServiceClient
            .GetBrandSuccessRateByReportDateAsync(
                Arg.Is(brandId),
                Arg.Is(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))),
                Arg.Any<CancellationToken>())
            .Returns(88m);

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getDaySuccessRate(brandId);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<DayDepositSuccessRateViewModel>(
            nodeModule,
            args: new object[] { brandId });

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(100m, actual.TodaySuccessRate);
        Assert.Equal(88m, actual.YesterdaySuccessRate);
    }

    [Fact]
    public async Task 查詢註冊玩家人數()
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

        _ = fakePlayerServiceClient
            .CountCurrentDayRegisterPlayersAsync(
                Arg.Is(brandId),
                Arg.Any<CancellationToken>())
            .Returns(168);

        _ = fakePlayerServiceClient
            .CountReportDateRegisterPlayersAsync(
                Arg.Is(brandId),
                Arg.Is(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))),
                Arg.Any<CancellationToken>())
            .Returns(88);

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getDayRegisterPlayersCountAsync(brandId);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<DayPlayerRegisterCountViewModel>(
            nodeModule,
            args: new object[] { brandId });

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(168, actual.TodayPlayerRegisterCount);
        Assert.Equal(88, actual.YesterdayPlayerRegisterCount);
    }

    [Fact]
    public async Task 查詢當日登入玩家人數()
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

        _ = fakePlayerServiceClient
            .CountCurrentDayLoginPlayersAsync(
                Arg.Is(brandId),
                Arg.Any<CancellationToken>())
            .Returns(168);

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getCurrentDayLoginPlayersCountAsync(brandId);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<decimal>(
            nodeModule,
            args: new object[] { brandId });

        // Assert
        Assert.Equal(168, actual);
    }
}
