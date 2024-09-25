using NUnit.Framework.Legacy;
using Tgs.Paging;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class DepartmentTests
{
    [Test]
    public async Task Count_Departments_Query()
    {
        var fakeQueries = Substitute.For<ITgsQueries<Department>>();

        var sut = new GetDepartmentsQueryService(fakeQueries);

        var paging = new Paging<DepartmentInfo>
        {
            RowCount = 10,
            Page = 0,
        };

        var resellerId = Guid.NewGuid();
        var licenseeId = Guid.NewGuid();
        var brandId = Guid.NewGuid();        
        var departmentId = Guid.NewGuid();

        var query = new GetDepartmentsQuery(
            resellerId,
            licenseeId,
            brandId,
            "",
            paging);

        _ = fakeQueries
            .GetQueries()
            .Returns(new Department[]
            {
                new()
                {
                    Id = departmentId,
                    Name = "DepartmentName",
                    ResellerId = resellerId,
                    Reseller = new Reseller()
                    {
                        Id = resellerId,
                        Name = "ResellerName"
                    },
                    LicenseeId = licenseeId,
                    Licensee = new Licensee()
                    {
                        Id = licenseeId,
                        Name = "LicenseeName"
                    },
                    BrandId = brandId,
                    Brand = new Brand
                    {
                        Id = brandId,
                        Name = "BrandName"
                    },
                    CreatedOn = DateTime.Now.AddMonths(-1),
                }
            }.AsQueryable());

        var actual = await sut.GetAsync(query);

        ClassicAssert.AreEqual(1, actual);
    }

    [Test]
    public async Task Get_Departments_Query()
    {
        var fakeQueries = Substitute.For<ITgsQueries<Department>>();

        var sut = new GetDepartmentsQueryService(fakeQueries);

        var paging = new Paging<DepartmentInfo>
        {
            RowCount = 10,
            Page = 0,
        };

        var resellerId = Guid.NewGuid();
        var licenseeId = Guid.NewGuid();
        var brandId = Guid.NewGuid();        
        var departmentId = Guid.NewGuid();

        var query = new GetDepartmentsQuery(
            resellerId,
            licenseeId,
            brandId,
            "",
            paging);

        _ = fakeQueries
            .GetQueries()
            .Returns(new Department[]
            {
                new()
                {
                    Id = departmentId,
                    Name = "DepartmentName",
                    ResellerId = resellerId,
                    Reseller = new Reseller()
                    {
                        Id = resellerId,
                        Name = "ResellerName"
                    },
                    LicenseeId = licenseeId,
                    Licensee = new Licensee()
                    {
                        Id = licenseeId,
                        Name = "LicenseeName"
                    },
                    BrandId = brandId,
                    Brand = new Brand
                    {
                        Id = brandId,
                        Name = "BrandName"
                    },
                    CreatedOn = DateTime.Now.AddMonths(-1),
                }
            }.AsQueryable());

        var actual = await sut.QueryAsync(query).ToArrayAsync();

        ClassicAssert.AreEqual(1, actual.Length);
    }
}
