using System.Collections.Immutable;
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
    public async Task 修改玩家身分審核()
    {
        var fakePlayerSerivceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new UpdatePlayerIdentityAuthenticationCommandService(fakePlayerSerivceClient);

        var command = new UpdatePlayerIdentityAuthenticationCommand("playerCode", true);

        await sut.ExecuteAsync(command);

        _ = fakePlayerSerivceClient.Received(1)
            .SetPlayerPropertiesAsync(Arg.Is(command.PlayerCode), Arg.Any<IImmutableDictionary<PlayerPropertyName, string?>>());
    }
}
