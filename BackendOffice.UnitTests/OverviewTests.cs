using NSubstitute;
using Tgs.BrandCertificate.Clients;
using Tgs.GameWagers.Client;
using Tgs.GameWagers.Common;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.BackOffice.ApplicationServices.Overview;
using TGS.Services.BackOffice.Domain.Enums;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class OverviewTests
{
    [Test]
    public async Task 查詢存提差()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new GetDayNetDepositQueryService(fakePaymentServiceClient);

        var brandId = Guid.NewGuid();
        var yesterday = DateOnly.FromDateTime(DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(8d)).AddDays(-1).DateTime);

        _ = fakePaymentServiceClient
            .GetBrandCurrentDayNetDepositAsync(Arg.Is(brandId))
            .Returns(168888m);

        _ = fakePaymentServiceClient
            .GetBrandReportDateNetDepositAsync(Arg.Is(brandId), Arg.Is(yesterday))
            .Returns(16888m);

        var actual = await sut.GetAsync(new GetDayNetDepositQuery(brandId));

        Assert.AreEqual(168888m, actual.Today);
        Assert.AreEqual(16888m, actual.Yesterday);
    }

    [Test]
    public async Task 查詢存款成功率()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new GetDaySuccessRateQueryService(fakePaymentServiceClient);

        var brandId = Guid.NewGuid();
        var yesterday = DateOnly.FromDateTime(DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(8d)).AddDays(-1).DateTime);

        _ = fakePaymentServiceClient
            .GetCurrentDayDepositSuccessRateAsync(Arg.Is(brandId))
            .Returns(100m);

        _ = fakePaymentServiceClient
            .GetBrandSuccessRateByReportDateAsync(Arg.Is(brandId), Arg.Is(yesterday))
            .Returns(88m);

        var actual = await sut.GetAsync(new GetDaySuccessRateQuery(brandId));

        Assert.AreEqual(100m, actual.Today);
        Assert.AreEqual(88m, actual.Yesterday);
    }

    [Test]
    public async Task 查詢當日十大玩家_存款()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeGameWagersClient = Substitute.For<IGameWagersClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new GetTopTenPlayerQueryService(fakePaymentServiceClient, fakeBrandServiceClient, fakeGameWagersClient, fakePlayerServiceClient);

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var brandCode = "GPI";
        var playerCodes = new string[] { "4uel5ke0uf3k" };

        _ = fakeBrandServiceClient
            .GetBrandInfoAsync(Arg.Is(brandId))
            .Returns(new BrandInfo
            {
                Code = brandCode
            });

        _ = fakePaymentServiceClient
            .GetCurrentDayTopTenDepositPlayersAsync(Arg.Is(brandId))
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
                Arg.Is<IEnumerable<string>>(x => x.SequenceEqual(playerCodes)))
            .Returns(new BrandWagerPlayerStatsInfo[]
            {
                new BrandWagerPlayerStatsInfo(Guid.NewGuid(),"GPCode",123, Guid.NewGuid(),"ResellerCode",Guid.NewGuid(),"LicenseeCode",brandId,brandCode,playerCodes[0],"username","CNY",1,100m,100m,0m,0m,100m,-100m)
            }.ToAsyncEnumerable());

        _ = fakePlayerServiceClient
            .QueryPlayerInfoListAsync(
                Arg.Is(brandId),
                Arg.Is<IEnumerable<string>>(p => p.SequenceEqual(playerCodes)))
            .Returns(new Tgs.PlayerService.Client.PlayerInfo[]
            {
                new Tgs.PlayerService.Client.PlayerInfo
                {
                    PlayerCode = playerCodes[0],
                    UserName = "username"
                }
            }.ToAsyncEnumerable());

        var actual = await sut
            .QueryAsync(new GetTopTenPlayerQuery(brandId, TopTenPlayerSortType.Deposit, false))
            .ToArrayAsync()
            .ConfigureAwait(false);

        Assert.IsNotNull(actual);
        Assert.AreEqual(1, actual.Length);
        Assert.AreEqual(playerCodes[0], actual[0].PlayerCode);
        Assert.AreEqual("username", actual[0].PlayerUserName);
        Assert.AreEqual(16888m, actual[0].DepositTotalAmount);
        Assert.AreEqual(88m, actual[0].WithdrawTotalAmount);
        Assert.AreEqual(100m, actual[0].Bet);
        Assert.AreEqual(0m, actual[0].Settle);
        Assert.AreEqual(-100m, actual[0].WinLoss);
    }

    [Test]
    public async Task 查詢當日十大玩家_投注()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeGameWagersClient = Substitute.For<IGameWagersClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new GetTopTenPlayerQueryService(fakePaymentServiceClient, fakeBrandServiceClient, fakeGameWagersClient, fakePlayerServiceClient);

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var brandCode = "GPI";
        var playerCodes = new string[] { "4uel5ke0uf3k" };

        _ = fakeBrandServiceClient
            .GetBrandInfoAsync(Arg.Is(brandId))
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
                Arg.Is(false))
            .Returns(new BrandWagerPlayerStatsInfo[]
            {
                new BrandWagerPlayerStatsInfo(Guid.NewGuid(),"GPCode",123, Guid.NewGuid(),"ResellerCode",Guid.NewGuid(),"LicenseeCode",brandId,brandCode,playerCodes[0],"username","CNY",1,100m,100m,0m,0m,100m,-100m)
            }.ToAsyncEnumerable());

        _ = fakePaymentServiceClient
            .GetCurrentDayDepositWithdrawTotalAmountAsync(
                Arg.Is<IEnumerable<string>>(p => p.SequenceEqual(playerCodes)))
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
                Arg.Is<IEnumerable<string>>(p => p.SequenceEqual(playerCodes)))
            .Returns(new Tgs.PlayerService.Client.PlayerInfo[]
            {
                new Tgs.PlayerService.Client.PlayerInfo
                {
                    PlayerCode = playerCodes[0],
                    UserName = "username"
                }
            }.ToAsyncEnumerable());

        var actual = await sut
            .QueryAsync(new GetTopTenPlayerQuery(brandId, TopTenPlayerSortType.Bet, false))
            .ToArrayAsync()
            .ConfigureAwait(false);

        Assert.IsNotNull(actual);
        Assert.AreEqual(1, actual.Length);
        Assert.AreEqual(playerCodes[0], actual[0].PlayerCode);
        Assert.AreEqual("username", actual[0].PlayerUserName);
        Assert.AreEqual(16888m, actual[0].DepositTotalAmount);
        Assert.AreEqual(88m, actual[0].WithdrawTotalAmount);
        Assert.AreEqual(100m, actual[0].Bet);
        Assert.AreEqual(0m, actual[0].Settle);
        Assert.AreEqual(-100m, actual[0].WinLoss);
    }

    [Test]
    public async Task 查詢當日十大玩家_派彩()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeGameWagersClient = Substitute.For<IGameWagersClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new GetTopTenPlayerQueryService(fakePaymentServiceClient, fakeBrandServiceClient, fakeGameWagersClient, fakePlayerServiceClient);

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var brandCode = "GPI";
        var playerCodes = new string[] { "4uel5ke0uf3k" };

        _ = fakeBrandServiceClient
            .GetBrandInfoAsync(Arg.Is(brandId))
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
                Arg.Is(false))
            .Returns(new BrandWagerPlayerStatsInfo[]
            {
                new BrandWagerPlayerStatsInfo(Guid.NewGuid(),"GPCode",123, Guid.NewGuid(),"ResellerCode",Guid.NewGuid(),"LicenseeCode",brandId,brandCode,playerCodes[0],"username","CNY",1,100m,100m,0m,0m,100m,-100m)
            }.ToAsyncEnumerable());

        _ = fakePaymentServiceClient
            .GetCurrentDayDepositWithdrawTotalAmountAsync(
                Arg.Is<IEnumerable<string>>(p => p.SequenceEqual(playerCodes)))
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
                Arg.Is<IEnumerable<string>>(p => p.SequenceEqual(playerCodes)))
            .Returns(new Tgs.PlayerService.Client.PlayerInfo[]
            {
                new Tgs.PlayerService.Client.PlayerInfo
                {
                    PlayerCode = playerCodes[0],
                    UserName = "username"
                }
            }.ToAsyncEnumerable());

        var actual = await sut
            .QueryAsync(new GetTopTenPlayerQuery(brandId, TopTenPlayerSortType.Settle, false))
            .ToArrayAsync()
            .ConfigureAwait(false);

        Assert.IsNotNull(actual);
        Assert.AreEqual(1, actual.Length);
        Assert.AreEqual(playerCodes[0], actual[0].PlayerCode);
        Assert.AreEqual("username", actual[0].PlayerUserName);
        Assert.AreEqual(16888m, actual[0].DepositTotalAmount);
        Assert.AreEqual(88m, actual[0].WithdrawTotalAmount);
        Assert.AreEqual(100m, actual[0].Bet);
        Assert.AreEqual(0m, actual[0].Settle);
        Assert.AreEqual(-100m, actual[0].WinLoss);
    }

    [Test]
    public async Task 查詢當日十大玩家_提款()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeGameWagersClient = Substitute.For<IGameWagersClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new GetTopTenPlayerQueryService(fakePaymentServiceClient, fakeBrandServiceClient, fakeGameWagersClient, fakePlayerServiceClient);

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var brandCode = "GPI";
        var playerCodes = new string[] { "4uel5ke0uf3k" };

        _ = fakeBrandServiceClient
            .GetBrandInfoAsync(Arg.Is(brandId))
            .Returns(new BrandInfo
            {
                Code = brandCode
            });

        _ = fakePaymentServiceClient
            .GetCurrentDayTopTenWithdrawPlayersAsync(Arg.Is(brandId))
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
                Arg.Is<IEnumerable<string>>(x => x.SequenceEqual(playerCodes)))
            .Returns(new BrandWagerPlayerStatsInfo[]
            {
                new BrandWagerPlayerStatsInfo(Guid.NewGuid(),"GPCode",123, Guid.NewGuid(),"ResellerCode",Guid.NewGuid(),"LicenseeCode",brandId,brandCode,playerCodes[0],"username","CNY",1,100m,100m,0m,0m,100m,-100m)
            }.ToAsyncEnumerable());

        _ = fakePlayerServiceClient
            .QueryPlayerInfoListAsync(
                Arg.Is(brandId),
                Arg.Is<IEnumerable<string>>(p => p.SequenceEqual(playerCodes)))
            .Returns(new Tgs.PlayerService.Client.PlayerInfo[]
            {
                new Tgs.PlayerService.Client.PlayerInfo
                {
                    PlayerCode = playerCodes[0],
                    UserName = "username"
                }
            }.ToAsyncEnumerable());

        var actual = await sut
            .QueryAsync(new GetTopTenPlayerQuery(brandId, TopTenPlayerSortType.Withdraw, false))
            .ToArrayAsync()
            .ConfigureAwait(false);

        Assert.IsNotNull(actual);
        Assert.AreEqual(1, actual.Length);
        Assert.AreEqual(playerCodes[0], actual[0].PlayerCode);
        Assert.AreEqual("username", actual[0].PlayerUserName);
        Assert.AreEqual(16888m, actual[0].DepositTotalAmount);
        Assert.AreEqual(88m, actual[0].WithdrawTotalAmount);
        Assert.AreEqual(100m, actual[0].Bet);
        Assert.AreEqual(0m, actual[0].Settle);
        Assert.AreEqual(-100m, actual[0].WinLoss);
    }

    [Test]
    public async Task 查詢當日十大玩家_輸贏升冪()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeGameWagersClient = Substitute.For<IGameWagersClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new GetTopTenPlayerQueryService(fakePaymentServiceClient, fakeBrandServiceClient, fakeGameWagersClient, fakePlayerServiceClient);

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var brandCode = "GPI";
        var playerCodes = new string[] { "4uel5ke0uf3k" };

        _ = fakeBrandServiceClient
            .GetBrandInfoAsync(Arg.Is(brandId))
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
                Arg.Is(true))
            .Returns(new BrandWagerPlayerStatsInfo[]
            {
                new BrandWagerPlayerStatsInfo(Guid.NewGuid(),"GPCode",123, Guid.NewGuid(),"ResellerCode",Guid.NewGuid(),"LicenseeCode",brandId,brandCode,playerCodes[0],"username","CNY",1,100m,100m,0m,0m,100m,-100m)
            }.ToAsyncEnumerable());

        _ = fakePaymentServiceClient
            .GetCurrentDayDepositWithdrawTotalAmountAsync(
                Arg.Is<IEnumerable<string>>(p => p.SequenceEqual(playerCodes)))
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
                Arg.Is<IEnumerable<string>>(p => p.SequenceEqual(playerCodes)))
            .Returns(new Tgs.PlayerService.Client.PlayerInfo[]
            {
                new Tgs.PlayerService.Client.PlayerInfo
                {
                    PlayerCode = playerCodes[0],
                    UserName = "username"
                }
            }.ToAsyncEnumerable());

        var actual = await sut
            .QueryAsync(new GetTopTenPlayerQuery(brandId, TopTenPlayerSortType.WinLoss, true))
            .ToArrayAsync()
            .ConfigureAwait(false);

        Assert.IsNotNull(actual);
        Assert.AreEqual(1, actual.Length);
        Assert.AreEqual(playerCodes[0], actual[0].PlayerCode);
        Assert.AreEqual("username", actual[0].PlayerUserName);
        Assert.AreEqual(16888m, actual[0].DepositTotalAmount);
        Assert.AreEqual(88m, actual[0].WithdrawTotalAmount);
        Assert.AreEqual(100m, actual[0].Bet);
        Assert.AreEqual(0m, actual[0].Settle);
        Assert.AreEqual(-100m, actual[0].WinLoss);
    }

    [Test]
    public async Task 查詢當日十大玩家_輸贏降冪()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeGameWagersClient = Substitute.For<IGameWagersClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new GetTopTenPlayerQueryService(fakePaymentServiceClient, fakeBrandServiceClient, fakeGameWagersClient, fakePlayerServiceClient);

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var brandCode = "GPI";
        var playerCodes = new string[] { "4uel5ke0uf3k" };

        _ = fakeBrandServiceClient
            .GetBrandInfoAsync(Arg.Is(brandId))
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
                Arg.Is(false))
            .Returns(new BrandWagerPlayerStatsInfo[]
            {
                new BrandWagerPlayerStatsInfo(Guid.NewGuid(),"GPCode",123, Guid.NewGuid(),"ResellerCode",Guid.NewGuid(),"LicenseeCode",brandId,brandCode,playerCodes[0],"username","CNY",1,100m,100m,0m,0m,100m,-100m)
            }.ToAsyncEnumerable());

        _ = fakePaymentServiceClient
            .GetCurrentDayDepositWithdrawTotalAmountAsync(
                Arg.Is<IEnumerable<string>>(p => p.SequenceEqual(playerCodes)))
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
                Arg.Is<IEnumerable<string>>(p => p.SequenceEqual(playerCodes)))
            .Returns(new Tgs.PlayerService.Client.PlayerInfo[]
            {
                new Tgs.PlayerService.Client.PlayerInfo
                {
                    PlayerCode = playerCodes[0],
                    UserName = "username"
                }
            }.ToAsyncEnumerable());

        var actual = await sut
            .QueryAsync(new GetTopTenPlayerQuery(brandId, TopTenPlayerSortType.WinLoss, false))
            .ToArrayAsync()
            .ConfigureAwait(false);

        Assert.IsNotNull(actual);
        Assert.AreEqual(1, actual.Length);
        Assert.AreEqual(playerCodes[0], actual[0].PlayerCode);
        Assert.AreEqual("username", actual[0].PlayerUserName);
        Assert.AreEqual(16888m, actual[0].DepositTotalAmount);
        Assert.AreEqual(88m, actual[0].WithdrawTotalAmount);
        Assert.AreEqual(100m, actual[0].Bet);
        Assert.AreEqual(0m, actual[0].Settle);
        Assert.AreEqual(-100m, actual[0].WinLoss);
    }

    [Test]
    public async Task 查詢當日十大遊戲_投注降冪()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeGameWagersClient = Substitute.For<IGameWagersClient>();
        var fakeBrandGameService = Substitute.For<IBrandGameService>();

        var sut = new GetTopTenGameQueryService(fakeBrandServiceClient, fakeGameWagersClient, fakeBrandGameService);

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var brandCode = "GPI";
        var sortType = TopTenGameSortType.Bet;
        var gameName = "Happy Red Packets";
        var gameProviderName = "KY";

        _ = fakeBrandServiceClient.GetBrandInfoAsync(Arg.Is(brandId))
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
                Arg.Is(false))
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


        var actual = await sut.QueryAsync(new GetTopTenGameQuery(brandId, sortType, false)).ToArrayAsync();

        Assert.AreEqual(1, actual.Length);
        Assert.AreEqual(gameName, actual[0].GameName);
        Assert.AreEqual(gameProviderName, actual[0].GameProviderName);
        Assert.AreEqual(100m, actual[0].Bet);
        Assert.AreEqual(-100m, actual[0].WinLoss);        
    }

    [Test]
    public async Task 查詢當日十大遊戲_輸贏降冪()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeGameWagersClient = Substitute.For<IGameWagersClient>();
        var fakeBrandGameService = Substitute.For<IBrandGameService>();

        var sut = new GetTopTenGameQueryService(fakeBrandServiceClient, fakeGameWagersClient, fakeBrandGameService);

        var brandId = Guid.NewGuid();
        var brandCode = "GPI";
        var sortType = TopTenGameSortType.WinLoss;
        var gameName = "Happy Red Packets";
        var gameProviderName = "KY";

        _ = fakeBrandServiceClient.GetBrandInfoAsync(Arg.Is(brandId))
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
                Arg.Is(false))
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


        var actual = await sut.QueryAsync(new GetTopTenGameQuery(brandId, sortType, false)).ToArrayAsync();

        Assert.AreEqual(1, actual.Length);
        Assert.AreEqual(gameName, actual[0].GameName);
        Assert.AreEqual(gameProviderName, actual[0].GameProviderName);
        Assert.AreEqual(100m, actual[0].Bet);
        Assert.AreEqual(-100m, actual[0].WinLoss);
    }

    [Test]
    public async Task 查詢當日十大遊戲_輸贏升冪()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeGameWagersClient = Substitute.For<IGameWagersClient>();
        var fakeBrandGameService = Substitute.For<IBrandGameService>();

        var sut = new GetTopTenGameQueryService(fakeBrandServiceClient, fakeGameWagersClient, fakeBrandGameService);

        var brandId = Guid.NewGuid();
        var brandCode = "GPI";
        var sortType = TopTenGameSortType.WinLoss;
        var gameName = "Happy Red Packets";
        var gameProviderName = "KY";

        _ = fakeBrandServiceClient.GetBrandInfoAsync(Arg.Is(brandId))
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
                Arg.Is(true))
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


        var actual = await sut.QueryAsync(new GetTopTenGameQuery(brandId, sortType, true)).ToArrayAsync();

        Assert.AreEqual(1, actual.Length);
        Assert.AreEqual(gameName, actual[0].GameName);
        Assert.AreEqual(gameProviderName, actual[0].GameProviderName);
        Assert.AreEqual(100m, actual[0].Bet);
        Assert.AreEqual(-100m, actual[0].WinLoss);
    }

    [Test]
    public async Task 查詢當日與昨天註冊人數()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new GetDayPlayerRegisterCountQueryService(fakePlayerServiceClient);

        var brandId = Guid.NewGuid();
        var yesterday = DateOnly.FromDateTime(DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(8d)).AddDays(-1).DateTime);

        _ = fakePlayerServiceClient
            .CountCurrentDayRegisterPlayersAsync(Arg.Is(brandId))
            .Returns(168);

        _ = fakePlayerServiceClient
            .CountReportDateRegisterPlayersAsync(Arg.Is(brandId), Arg.Is(yesterday))
            .Returns(88);

        var actual = await sut.GetAsync(new GetDayPlayerRegisterCountQuery(brandId));

        Assert.AreEqual(168, actual.Today);
        Assert.AreEqual(88, actual.Yesterday);
    }
}
