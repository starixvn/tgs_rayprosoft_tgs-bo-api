using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class GameTests
{
    [Test]
    public async Task Get_Brand_Game_List()
    {
        var moqBackOfficeRepository = new Mock<IBackOfficeRepository>();

        var sut = new GetAllGamesByBrandIdQueryService(moqBackOfficeRepository.Object);

        var query = new GetAllGamesByBrandIdQuery(Guid.NewGuid());

        _ = moqBackOfficeRepository
            .Setup(fake => fake.QueryAllGamesAsync(It.IsAny<Guid>()))
            .Returns(Array.Empty<GameData>().ToAsyncEnumerable());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        moqBackOfficeRepository.Verify(
            fake => fake.QueryAllGamesAsync(It.Is<Guid>(x => x == query.BrandId)),
            Times.Exactly(1));
    }

    [Test]
    public async Task Get_Game_List()
    {
        var moqGameRepository = new Mock<IGameRepository>();

        var sut = new GetAllGamesByGameProviderIdQueryService(moqGameRepository.Object);

        var query = new GetAllGamesByGameProviderIdQuery(Guid.NewGuid());

        _ = moqGameRepository
            .Setup(fake => fake.QueryAllGamesAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .Returns(Array.Empty<GameInfo>().ToAsyncEnumerable());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        moqGameRepository.Verify(
            fake => fake.QueryAllGamesAsync(
                It.Is<Guid>(x => x == query.GameProviderId),
                It.IsAny<CancellationToken>()),
            Times.Exactly(1));
    }
}
