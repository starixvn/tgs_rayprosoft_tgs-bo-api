using NSubstitute;
using Tgs.StationLetterService.Client;
using TGS.Services.BackOffice.ApplicationServices.StationLetters;

namespace TGS.Tests.BackendOffice.UnitTests;

public class StationLetterTests
{
    [Test]
    public async Task 取得Brand收到的站內信()
    {
        var fakeStationLetterClient = Substitute.For<IStationLetterClient>();

        var sut = new GetBrandLettersQueryService(fakeStationLetterClient);

        var query = new GetBrandLettersQuery("GPI");

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakeStationLetterClient.Received(1)
            .ListStationLettersAsync(Arg.Is(query.BrandCode));
    }
}
