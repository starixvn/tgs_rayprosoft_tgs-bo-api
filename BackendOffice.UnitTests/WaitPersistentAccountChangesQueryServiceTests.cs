using System.Collections.Immutable;
using NSubstitute;
using Tgs.CashService.Client;
using Tgs.PlayerService.Client;
using TGS.Services.BackOffice.ApplicationServices.Wallets;

namespace TGS.Tests.BackendOffice.UnitTests;

public class WaitPersistentAccountChangesQueryServiceTests
{
    [Test]
    public async Task 查詢玩家尚未寫回資料庫的帳變資料()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        var sut = new WaitPersistentAccountChangesQueryService(
            fakePlayerServiceClient,
            fakeCashServiceClient);

        var query = new WaitPersistentAccountChangesQuery(
            "test1",
            new PagingConditions(1, 10));

        var walletId = Guid.NewGuid();

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(
            Arg.Is(query.PlayerCode))
            .Returns(new Dictionary<string, string>
            {
                ["DefaultWalletId"] = walletId.ToString()
            }.ToImmutableDictionary());

        _ = await sut.GetAsync(query);

        _ = fakeCashServiceClient.Received(1)
            .QueryInRedisTransactionLogsAsync(Arg.Is(walletId), Arg.Is(query.Paging));
    }
}
