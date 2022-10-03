using System;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Application.Services;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class BrandMaintenanceTests
{
    [Test]
    public async Task User_Can_Update_All_Brand_To_Maintenance_Status()
    {
        var moqBrandSerivce = new Mock<IBrandService>();

        var sut = new UpdateAllBrandMaintenanceStatusCommandService(moqBrandSerivce.Object);

        var command = new UpdateAllBrandMaintenanceStatusCommand("editorUserName");

        var list = new Brand[]
        {
            new Brand
            {
                Status = BrandStatus.Active
            },
            new Brand
            {
                Status = BrandStatus.Active
            }
        }
           .AsQueryable()
           .BuildMock();

        _ = moqBrandSerivce
            .Setup(b => b.GetBrandQueries(It.IsAny<Func<IQueryable<Brand>, IQueryable<Brand>>>(), false))
            .Returns(list);

        await sut.ExecuteAsync(command);

        moqBrandSerivce.Verify(
            fake => fake.ChangeStatusAsync(
                It.IsAny<Brand>(),
                It.IsAny<UpdateBrandStatusRequest>()),
            Times.Exactly(2));
    }
}
