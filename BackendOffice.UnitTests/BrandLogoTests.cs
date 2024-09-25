using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class BrandLogoTests
{
    [Test]
    public async Task Can_Get_Brand_LogoInfo()
    {
        var moqBackOfficeRepository = Substitute.For<IBackOfficeRepository>();

        var sut = new GetBrandLogoQueryService(moqBackOfficeRepository);

        var query = new GetBrandLogoQuery(Guid.NewGuid());

        _ = await sut.GetAsync(query);

        _ = moqBackOfficeRepository.Received(1).GetBrandLogoInfoAsync(Arg.Is<Guid>(x => x == query.BrandId));
    }

    [Test]
    public async Task Can_Modify_Brand_LogoInfo()
    {
        var moqBackOfficeRepository = Substitute.For<IBackOfficeRepository>();

        var sut = new ModifyBrandLogoInfoCommandService(moqBackOfficeRepository);

        var command = new ModifyBrandLogoInfoCommand(
            Guid.NewGuid(),
            new("https://test.com/pic/aaa.jpg"),
            new("https://test.com/pic/bbb.jpg"),
            new("https://test.com/pic/loading.gif"),
            "User1");

        await sut.ExecuteAsync(command);

        _ = moqBackOfficeRepository.Received(1).UpdateBrandLogoInfoAsync(
            Arg.Any<BrandLogoInfo>(),
            Arg.Is<string>(x => x == command.Modifier));
    }
}