using System.Collections.Immutable;
using System.Text.Json;
using NSubstitute;
using Tgs.CashService.Client;
using Tgs.PlayerService.Client;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Redis;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Queries.PlayerHandlers;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.ApplicationServices.Players;
using BoEnums = TGS.Services.BackOffice.Domain.Enums;

namespace TGS.Tests.BackendOffice.UnitTests;

internal class PlayerTests
{
    [Test]
    public async Task 以多筆UserName查詢玩家資料()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var brandId = Guid.NewGuid();
        var userNames = new[] { "username1", "username2" };

        var playerCodes = new[] { "playerCode1", "playerCode2" };

        var sut = new PlayerInfosByUserNamesQueryService(fakePlayerServiceClient);

        var expected = new[]
        {
            new PlayerInfo
            {
                BrandId = brandId,
                PlayerCode = "playerCode1",
                UserName = "username1",
            },
            new PlayerInfo
            {
                BrandId = brandId,
                PlayerCode = "playerCode2",
                UserName = "username2",
            }
        };

        _ = fakePlayerServiceClient.GetPlayerCodesByUserNamesAsync(
                Arg.Is(brandId),
                Arg.Is<IEnumerable<string>>(x => userNames.SequenceEqual(x)),
                Arg.Any<CancellationToken>())
            .Returns(playerCodes.ToAsyncEnumerable());

        _ = fakePlayerServiceClient.QueryPlayerInfoListAsync(
                Arg.Is(brandId),
                Arg.Is<IEnumerable<string>>(x => playerCodes.SequenceEqual(x)),
                Arg.Any<CancellationToken>())
            .Returns(expected.ToAsyncEnumerable());

        var query = new PlayerInfosByUserNamesQuery(brandId, userNames);

        var actual = await sut.QueryAsync(query).ToArrayAsync();

        Assert.NotNull(actual);
        CollectionAssert.AreEquivalent(expected, actual);
    }

    [Test]
    public async Task 以多筆UserName查詢玩家資料_若找不到玩家則回傳空資料()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var brandId = Guid.NewGuid();
        var userNames = new[] { "username1", "username2" };

        var sut = new PlayerInfosByUserNamesQueryService(fakePlayerServiceClient);

        _ = fakePlayerServiceClient.GetPlayerCodesByUserNamesAsync(
                Arg.Is(brandId),
                Arg.Is<IEnumerable<string>>(x => userNames.SequenceEqual(x)),
                Arg.Any<CancellationToken>())
            .Returns(Array.Empty<string>().ToAsyncEnumerable());

        var query = new PlayerInfosByUserNamesQuery(brandId, userNames);

        var actual = await sut.QueryAsync(query).ToArrayAsync();

        Assert.AreEqual(0, actual.Length);

        _ = fakePlayerServiceClient
            .DidNotReceive()
            .QueryPlayerInfoListAsync(
                Arg.Is(brandId),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 依據UserName前置取得玩家PlayerCode列表()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new ListPlayerCodesByUserNamePrefixQueryService(fakePlayerServiceClient);

        var query = new ListPlayerCodesByUserNamePrefixQuery(Guid.NewGuid(), "prefix");

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakePlayerServiceClient.Received(1)
            .ListPlayerCodesByUserNameAsync(
                Arg.Is(query.BrandId),
                Arg.Is(query.UserNamePrefix));
    }

    [Test]
    public async Task 取得玩家帳變紀錄_不指定交易類型時_須排除WithdrawCommit和TransferCommit查詢結果()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        var sut = new AccountChangesQueryService(fakePlayerServiceClient, fakeCashServiceClient);

        var playerCode = "4ucmqplxb029";
        var playerWalletId = Guid.Parse("36a6cdef-094f-eb11-8d88-70b5e82b6b8d");

        _ = fakePlayerServiceClient
            .GetPlayerPropertiesAsync(Arg.Is(playerCode))
            .Returns(new Dictionary<string, string>
            {
                { "DefaultWalletId", playerWalletId.ToString() }
            }.ToImmutableDictionary());

        var query = new AccountChangesQuery(
            playerCode,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow,
            null,
            new PagingConditions(1, 20));

        var actual = await sut.GetAsync(query);

        var queryTransTypes = Enum
                .GetValues(typeof(BoEnums.TransactionType))
                .Cast<BoEnums.TransactionType>()
                .Where(x => x is not BoEnums.TransactionType.WithdrawCommit
                    and not BoEnums.TransactionType.TransferCommit)
                .Select(x => (int)x)
                .ToArray();

        _ = fakeCashServiceClient.Received(1)
            .QueryTransactionLogsAsync(
                Arg.Is(playerWalletId),
                Arg.Is(query.From),
                Arg.Is(query.To),
                Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(queryTransTypes)),
                Arg.Is(query.Paging));
    }

    [Test]
    public async Task 取得玩家帳變紀錄_指定交易類型存款時_須包含Deposit和Deposited查詢結果()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        var sut = new AccountChangesQueryService(fakePlayerServiceClient, fakeCashServiceClient);

        var playerCode = "4ucmqplxb029";
        var playerWalletId = Guid.Parse("36a6cdef-094f-eb11-8d88-70b5e82b6b8d");

        _ = fakePlayerServiceClient
            .GetPlayerPropertiesAsync(Arg.Is(playerCode))
            .Returns(new Dictionary<string, string>
            {
                { "DefaultWalletId", playerWalletId.ToString() }
            }.ToImmutableDictionary());

        var query = new AccountChangesQuery(
            playerCode,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow,
            BoEnums.TransactionType.Deposit,
            new PagingConditions(1, 20));

        var actual = await sut.GetAsync(query);

        var queryTransTypes = new int[]
        {
            (int)BoEnums.TransactionType.Deposit,
            (int)BoEnums.TransactionType.Deposited
        };

        _ = fakeCashServiceClient.Received(1)
            .QueryTransactionLogsAsync(
                Arg.Is(playerWalletId),
                Arg.Is(query.From),
                Arg.Is(query.To),
                Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(queryTransTypes)),
                Arg.Is(query.Paging));
    }

    [Test]
    public async Task 取得玩家帳變紀錄_指定交易類型取消提款時_須包含CancelWithdrawal和WithdrawRollback查詢結果()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        var sut = new AccountChangesQueryService(fakePlayerServiceClient, fakeCashServiceClient);

        var playerCode = "4ucmqplxb029";
        var playerWalletId = Guid.Parse("36a6cdef-094f-eb11-8d88-70b5e82b6b8d");

        _ = fakePlayerServiceClient
            .GetPlayerPropertiesAsync(Arg.Is(playerCode))
            .Returns(new Dictionary<string, string>
            {
                { "DefaultWalletId", playerWalletId.ToString() }
            }.ToImmutableDictionary());

        var query = new AccountChangesQuery(
            playerCode,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow,
            BoEnums.TransactionType.CancelWithdrawal,
            new PagingConditions(1, 20));

        var actual = await sut.GetAsync(query);

        var queryTransTypes = new int[]
        {
            (int)BoEnums.TransactionType.CancelWithdrawal,
            (int)BoEnums.TransactionType.WithdrawRollback
        };

        _ = fakeCashServiceClient.Received(1)
            .QueryTransactionLogsAsync(
                Arg.Is(playerWalletId),
                Arg.Is(query.From),
                Arg.Is(query.To),
                Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(queryTransTypes)),
                Arg.Is(query.Paging));
    }

    [Test]
    public async Task 取得玩家帳變紀錄_指定交易類型提款時_須包含Withdrawal和WithdrawLock查詢結果()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        var sut = new AccountChangesQueryService(fakePlayerServiceClient, fakeCashServiceClient);

        var playerCode = "4ucmqplxb029";
        var playerWalletId = Guid.Parse("36a6cdef-094f-eb11-8d88-70b5e82b6b8d");

        _ = fakePlayerServiceClient
            .GetPlayerPropertiesAsync(Arg.Is(playerCode))
            .Returns(new Dictionary<string, string>
            {
                { "DefaultWalletId", playerWalletId.ToString() }
            }.ToImmutableDictionary());

        var query = new AccountChangesQuery(
            playerCode,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow,
            BoEnums.TransactionType.Withdrawal,
            new PagingConditions(1, 20));

        var actual = await sut.GetAsync(query);

        var queryTransTypes = new int[]
        {
            (int)BoEnums.TransactionType.Withdrawal,
            (int)BoEnums.TransactionType.WithdrawLock
        };

        _ = fakeCashServiceClient.Received(1)
            .QueryTransactionLogsAsync(
                Arg.Is(playerWalletId),
                Arg.Is(query.From),
                Arg.Is(query.To),
                Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(queryTransTypes)),
                Arg.Is(query.Paging));
    }

    [Test]
    public async Task 取得玩家帳變紀錄總金額_不指定交易類型時_須排除WithdrawCommit和TransferCommit查詢結果()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        var sut = new AccountChangesTotalAmountQueryService(fakePlayerServiceClient, fakeCashServiceClient);

        var playerCode = "4ucmqplxb029";
        var playerWalletId = Guid.Parse("36a6cdef-094f-eb11-8d88-70b5e82b6b8d");

        _ = fakePlayerServiceClient
            .GetPlayerPropertiesAsync(Arg.Is(playerCode))
            .Returns(new Dictionary<string, string>
            {
                { "DefaultWalletId", playerWalletId.ToString() }
            }.ToImmutableDictionary());

        var query = new AccountChangesTotalAmountQuery(
            playerCode,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow,
            null);

        var actual = await sut.GetAsync(query);

        var queryTransTypes = Enum
                .GetValues(typeof(BoEnums.TransactionType))
                .Cast<BoEnums.TransactionType>()
                .Where(x => x is not BoEnums.TransactionType.WithdrawCommit
                    and not BoEnums.TransactionType.TransferCommit)
                .Select(x => (int)x)
                .ToArray();

        _ = fakeCashServiceClient.Received(1)
            .QueryTransactionTotalAmountAsync(
                Arg.Is(playerWalletId),
                Arg.Is(query.From),
                Arg.Is(query.To),
                Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(queryTransTypes)));
    }

    [Test]
    public async Task 取得玩家帳變紀錄總金額_指定交易類型存款時_須包含Deposit和Deposited查詢結果()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        var sut = new AccountChangesTotalAmountQueryService(fakePlayerServiceClient, fakeCashServiceClient);

        var playerCode = "4ucmqplxb029";
        var playerWalletId = Guid.Parse("36a6cdef-094f-eb11-8d88-70b5e82b6b8d");

        _ = fakePlayerServiceClient
            .GetPlayerPropertiesAsync(Arg.Is(playerCode))
            .Returns(new Dictionary<string, string>
            {
                { "DefaultWalletId", playerWalletId.ToString() }
            }.ToImmutableDictionary());

        var query = new AccountChangesTotalAmountQuery(
            playerCode,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow,
            BoEnums.TransactionType.Deposit);

        var actual = await sut.GetAsync(query);

        var queryTransTypes = new int[]
        {
            (int) BoEnums.TransactionType.Deposit,
            (int) BoEnums.TransactionType.Deposited,
        };

        _ = fakeCashServiceClient.Received(1)
            .QueryTransactionTotalAmountAsync(
                Arg.Is(playerWalletId),
                Arg.Is(query.From),
                Arg.Is(query.To),
                Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(queryTransTypes)));
    }

    [Test]
    public async Task 取得玩家帳變紀錄總金額_指定交易類型提款時_須包含Withdrawal和WithdrawLock查詢結果()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        var sut = new AccountChangesTotalAmountQueryService(fakePlayerServiceClient, fakeCashServiceClient);

        var playerCode = "4ucmqplxb029";
        var playerWalletId = Guid.Parse("36a6cdef-094f-eb11-8d88-70b5e82b6b8d");

        _ = fakePlayerServiceClient
            .GetPlayerPropertiesAsync(Arg.Is(playerCode))
            .Returns(new Dictionary<string, string>
            {
                { "DefaultWalletId", playerWalletId.ToString() }
            }.ToImmutableDictionary());

        var query = new AccountChangesTotalAmountQuery(
            playerCode,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow,
            BoEnums.TransactionType.Withdrawal);

        var actual = await sut.GetAsync(query);

        var queryTransTypes = new int[]
        {
            (int) BoEnums.TransactionType.Withdrawal,
            (int) BoEnums.TransactionType.WithdrawLock,
        };

        _ = fakeCashServiceClient.Received(1)
            .QueryTransactionTotalAmountAsync(
                Arg.Is(playerWalletId),
                Arg.Is(query.From),
                Arg.Is(query.To),
                Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(queryTransTypes)));
    }

    [Test]
    public async Task 查詢玩家歷史紀錄()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new PlayerHistoriesQueryService(fakePlayerServiceClient);

        var playerCode = "playerCode";
        var from = DateTime.Now.AddDays(-14);
        var to = DateTime.Now;

        _ = fakePlayerServiceClient
            .CountPlayerHistoriesAsync(new GetPlayerHistoriesQueryConditions(playerCode, from, to))
            .Returns(20);

        _ = await sut.QueryAsync(new PlayerHistoriesQuery(playerCode, from, to)).ToArrayAsync();

        var conditions = new GetPlayerHistoriesQueryConditions(playerCode, from, to);

        _ = fakePlayerServiceClient
            .Received(1)
            .GetPlayerHistoriesAsync(Arg.Is(conditions), Arg.Is(20));
    }

    [Test]
    public async Task 查詢玩家歷史紀錄內容()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new PlayerHistoryQueryService(fakePlayerServiceClient);

        var eventId = Guid.NewGuid();

        _ = fakePlayerServiceClient
            .GetPlayerHistoryAsync(Arg.Is(eventId))
            .Returns(new PlayerEventInfo()
            {
                Action = EventAction.Create,
                ChangeSets = JsonDocument.Parse("{}"),
                CreatedOn = DateTime.Now.AddDays(-1),
            });

        var actual = await sut.GetAsync(new PlayerHistoryQuery(eventId));

        Assert.AreEqual(EventAction.Create, actual.Action);
    }

    [Test]
    public async Task 查詢玩家歷史紀錄總筆數()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new PlayerHistoriesQueryService(fakePlayerServiceClient);

        var playerCode = "playerCode";
        var from = DateTime.Now.AddDays(-14);
        var to = DateTime.Now;

        _ = fakePlayerServiceClient
            .CountPlayerHistoriesAsync(new GetPlayerHistoriesQueryConditions(playerCode, from, to))
            .Returns(20);

        _ = await sut.GetAsync(new PlayerHistoriesQuery(playerCode, from, to));

        var conditions = new GetPlayerHistoriesQueryConditions(playerCode, from, to);

        _ = fakePlayerServiceClient
            .Received(1)
            .CountPlayerHistoriesAsync(Arg.Is(conditions));
    }

    [Test]
    public async Task 修改玩家身分審核()
    {
        var fakePlayerSerivceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new UpdatePlayerIdentityAuthenticationCommandService(fakePlayerSerivceClient);

        var command = new UpdatePlayerIdentityAuthenticationCommand("playerCode", true, "6a379883-d8d2-4ffb-3d05-08d9262ce1a5");

        await sut.ExecuteAsync(command);

        _ = fakePlayerSerivceClient.Received(1)
            .SetPlayerPropertiesAsync(
                Arg.Is(command.PlayerCode),
                Arg.Any<IImmutableDictionary<PlayerPropertyName, string?>>(),
                Arg.Is(command.ModifyUser),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 修改玩家密碼()
    {
        var fakePlayerSerivceClient = Substitute.For<IPlayerServiceClient>();
        var fakeRedisCacheManager = Substitute.For<IRedisCacheManager>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new SetPlayerPasswordCommandService(fakePlayerSerivceClient, fakeRedisCacheManager, fakeTgsRepository);

        var playerCode = "playerCode";
        var password = "newPassword";
        var modifyUserId = "6a379883-d8d2-4ffb-3d05-08d9262ce1a5";
        var modifyUsername = "username";

        var command = new SetPlayerPasswordCommand(playerCode, password, modifyUserId, modifyUsername);
        await sut.ExecuteAsync(command);

        _ = fakePlayerSerivceClient
            .Received(1)
            .SetPlayerPasswordAsync(Arg.Is(playerCode), Arg.Is(password), Arg.Is(modifyUsername), Arg.Is(modifyUserId));

        _ = fakeRedisCacheManager
            .Received(1)
            .ClearCacheAsync(Arg.Is(RedisCacheType.Player), Arg.Is(playerCode));

        _ = fakeTgsRepository.Received(1)
            .PlayerAuditLogs
            .AddAsync(Arg.Any<PlayerAuditLog>());

        _ = fakeTgsRepository.Received(1)
            .SaveChangesAsync();
    }
}
