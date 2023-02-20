using NSubstitute;
using Tgs.PlayerService.Client;
using TGS.Services.BackOffice.ApplicationServices;

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
}