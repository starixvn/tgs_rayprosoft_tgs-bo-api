using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class BrandLogoTests
{
    [Test]
    public async Task Can_Get_Brand_LogoInfo()
    {
        var moqBackOfficeRepository = new Mock<IBackOfficeRepository>();

        var sut = new GetBrandLogoQueryService(moqBackOfficeRepository.Object);

        var query = new GetBrandLogoQuery(Guid.NewGuid());

        _ = await sut.GetAsync(query);

        moqBackOfficeRepository.Verify(
            fake => fake.GetBrandLogoInfoAsync(It.Is<Guid>(x => x == query.BrandId)),
            Times.Exactly(1));
    }

    [Test]
    public async Task Can_Modify_Brand_LogoInfo()
    {
        var moqBackOfficeRepository = new Mock<IBackOfficeRepository>();

        var sut = new ModifyBrandLogoInfoCommandService(moqBackOfficeRepository.Object);

        var command = new ModifyBrandLogoInfoCommand(
            Guid.NewGuid(),
            new Uri("https://test.com/pic/aaa.jpg"),
            new Uri("https://test.com/pic/bbb.jpg"),
            new Uri("https://test.com/pic/loading.gif"),
            "User1");

        await sut.ExecuteAsync(command);

        moqBackOfficeRepository.Verify(
            fake => fake.UpdateBrandLogoInfoAsync(
                It.IsAny<BrandLogoInfo>(),
                It.Is<string>(x => x == command.Modifier)),
            Times.Exactly(1));
    }
}
