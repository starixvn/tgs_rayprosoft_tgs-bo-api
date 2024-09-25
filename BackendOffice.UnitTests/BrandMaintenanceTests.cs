using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.ApplicationServices;
using BrandListQuery = TGS.Services.BackOffice.ApplicationServices.BrandListQuery;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class BrandMaintenanceTests
{
    [Test]
    public async Task User_Can_Update_All_Brand_To_Maintenance_Status()
    {
        var fakeBrandListQueryService = Substitute.For<IQueryService<BrandListQuery, BrandListItem>>();
        var fakeUpdateBrandStatusCommandService = Substitute.For<ICommandService<UpdateBrandStatusCommand>>();

        var sut = new UpdateAllBrandMaintenanceStatusCommandService(
            fakeBrandListQueryService, 
            fakeUpdateBrandStatusCommandService);

        var command = new UpdateAllBrandMaintenanceStatusCommand("editorUserName");

        var brands = new[]
        {
            new BrandListItem(
                Guid.NewGuid(),
                Tgs.BrandCertificate.BrandStatus.Active,
                null,
                null,
                DateTime.Parse("2023-01-01 00:00:00"),
                DateTime.Parse("2023-01-01 00:00:00")),
            new BrandListItem(
                Guid.NewGuid(),
                Tgs.BrandCertificate.BrandStatus.Active,
                null,
                null,
                DateTime.Parse("2023-01-10 00:00:00"),
                DateTime.Parse("2023-01-10 00:00:00"))
        };

        _ = fakeBrandListQueryService.QueryAsync(
                Arg.Is<BrandListQuery>(x => x.Status == BrandStatus.Active),
                Arg.Any<CancellationToken>())
            .Returns(brands.ToAsyncEnumerable());

        await sut.ExecuteAsync(command);
        
        _ = fakeUpdateBrandStatusCommandService.Received(2)
            .ExecuteAsync(
                Arg.Is<UpdateBrandStatusCommand>(x => x.Status == BrandStatus.Maintenance),
                Arg.Any<CancellationToken>());
    }
}
