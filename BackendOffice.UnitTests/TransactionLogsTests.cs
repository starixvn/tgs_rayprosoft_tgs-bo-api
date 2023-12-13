using System.Collections.Immutable;
using NSubstitute;
using Tgs.CashService.Client;
using Tgs.PlayerService.Client;
using TGS.Services.BackOffice.ApplicationServices.Wallets;
using BoEnums = TGS.Services.BackOffice.Domain.Enums;

namespace TGS.Tests.BackendOffice.UnitTests;

public class TransactionLogsTests
{
    [Test]
    public async Task 取得帳變紀錄_不指定交易類型時_須排除部分查詢結果()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        var sut = new AccountChangesQueryService(fakeCashServiceClient);

        var playerWalletId = Guid.Parse("36a6cdef-094f-eb11-8d88-70b5e82b6b8d");

        var query = new AccountChangesQuery(
            playerWalletId,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow,
            null,
            new PagingConditions(1, 20));

        var actual = await sut.GetAsync(query);

        var excludeTypes = new int[] {
            (int)BoEnums.TransactionType.WithdrawCommit,
            (int)BoEnums.TransactionType.TransferCommit,
            (int)BoEnums.TransactionType.Receive
        };

        _ = fakeCashServiceClient.Received(1)
            .QueryTransactionLogsAsync(
                Arg.Is(playerWalletId),
                Arg.Is(query.From),
                Arg.Is(query.To),
                Arg.Is<IEnumerable<int>>(x => !x.Intersect(excludeTypes).Any()),
                Arg.Is(query.Paging));
    }

    [Test]
    public async Task 取得帳變紀錄_指定交易類型存款時_須包含Deposit和Deposited查詢結果()
    {
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        var sut = new AccountChangesQueryService(fakeCashServiceClient);

        var playerWalletId = Guid.Parse("36a6cdef-094f-eb11-8d88-70b5e82b6b8d");

        var query = new AccountChangesQuery(
            playerWalletId,
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
    public async Task 取得帳變紀錄_指定交易類型取消提款時_須包含CancelWithdrawal和WithdrawRollback查詢結果()
    {
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        var sut = new AccountChangesQueryService(fakeCashServiceClient);

        var playerWalletId = Guid.Parse("36a6cdef-094f-eb11-8d88-70b5e82b6b8d");

        var query = new AccountChangesQuery(
            playerWalletId,
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
    public async Task 取得帳變紀錄_指定交易類型提款時_須包含Withdrawal和WithdrawLock查詢結果()
    {
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        var sut = new AccountChangesQueryService(fakeCashServiceClient);

        var playerWalletId = Guid.Parse("36a6cdef-094f-eb11-8d88-70b5e82b6b8d");

        var query = new AccountChangesQuery(
            playerWalletId,
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
    public async Task 取得玩家帳變紀錄總金額_不指定交易類型時_須排除部分交易類型查詢結果()
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

        var excludeTypes = new int[] {
            (int)BoEnums.TransactionType.WithdrawCommit,
            (int)BoEnums.TransactionType.TransferCommit,
            (int)BoEnums.TransactionType.Receive
        };

        _ = fakeCashServiceClient.Received(1)
            .QueryTransactionTotalAmountAsync(
                Arg.Is(playerWalletId),
                Arg.Is(query.From),
                Arg.Is(query.To),
                Arg.Is<IEnumerable<int>>(x => !x.Intersect(excludeTypes).Any()));
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
}
