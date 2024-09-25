using Tgs.PlayerService.Client;
using TGS.Services.BackOffice.ApplicationServices.Players;

namespace TGS.Tests.BackendOffice.UnitTests;

public class GetPlayerInfoByPlayerCodeQueryServiceTests
{
    [Test]
    public async Task 以PlayerCode取得玩家資訊()
    {
        var fakePlayerServiceClient = NSubstitute.Substitute.For<IPlayerServiceClient>();

        var sut = new GetPlayerInfoByPlayerCodeQueryService(fakePlayerServiceClient);

        var query = new GetPlayerInfoByPlayerCodeQuery("test1");

        _ = await sut.GetAsync(query);

        _ = fakePlayerServiceClient.Received(1)
            .GetPlayerInfoAsync(Arg.Is(query.PlayerCode));
    }
}
