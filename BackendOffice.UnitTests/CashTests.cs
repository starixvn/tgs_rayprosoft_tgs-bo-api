using System.Collections.Immutable;
using NSubstitute;
using Tgs.CashService.Client;
using Tgs.PlayerService.Client;
using TGS.Services.BackOffice.ApplicationServices.Wallets;

namespace TGS.Tests.BackendOffice.UnitTests;

public class CashTests
{
    [Test]
    public async Task 取得玩家的錢包餘額()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        var sut = new GetPlayerBalanceQueryService(
            fakePlayerServiceClient,
            fakeCashServiceClient);

        var query = new GetPlayerBalanceQuery("test1");

        var walletId = Guid.NewGuid();

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(query.PlayerCode))
            .Returns(new Dictionary<string, string>
            {
                ["DefaultWalletId"] = walletId.ToString()
            }.ToImmutableDictionary());

        _ = await sut.GetAsync(query);

        _ = fakeCashServiceClient.Received(1)
            .GetBalanceAsync(Arg.Is(walletId));
    }
}
