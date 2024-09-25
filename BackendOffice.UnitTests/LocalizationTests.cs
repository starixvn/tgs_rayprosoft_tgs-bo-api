using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests;

public class LocalizationTests
{
    [Test]
    public async Task Get_LocalizationStrings_By_CultureCode()
    {
        var fakeBackOfficeRepository = Substitute.For<IBackOfficeRepository>();

        var sut = new GetAllLocalizationStringsQueryService(fakeBackOfficeRepository);

        var query = new GetAllLocalizationStringsQuery("en-US");

        _ = await sut.GetAsync(query);

        _ = fakeBackOfficeRepository.Received(1)
            .QueryAllLocalizationStringsAsync(Arg.Is(query.CultureCode));
    }
}
