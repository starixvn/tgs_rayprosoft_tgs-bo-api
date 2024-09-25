using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class GameTests
{
    [Test]
    public async Task Get_Brand_Game_List()
    {
        var moqBackOfficeRepository = Substitute.For<IBackOfficeRepository>();

        var sut = new GetAllGamesByBrandIdQueryService(moqBackOfficeRepository);

        var query = new GetAllGamesByBrandIdQuery(Guid.NewGuid());

        _ = moqBackOfficeRepository.QueryAllGamesAsync(Arg.Any<Guid>())
            .Returns(Array.Empty<GameData>().ToAsyncEnumerable());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = moqBackOfficeRepository.Received(1).QueryAllGamesAsync(Arg.Is<Guid>(x => x == query.BrandId));
    }

    [Test]
    public async Task Get_Game_List()
    {
        var moqGameRepository = Substitute.For<IGameRepository>();

        var sut = new GetAllGamesByGameProviderIdQueryService(moqGameRepository);

        var query = new GetAllGamesByGameProviderIdQuery(Guid.NewGuid());

        _ = moqGameRepository.QueryAllGamesAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns(Array.Empty<GameInfo>().ToAsyncEnumerable());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = moqGameRepository.Received(1).QueryAllGamesAsync(
            Arg.Is<Guid>(x => x == query.GameProviderId),
            Arg.Any<CancellationToken>());
    }
}