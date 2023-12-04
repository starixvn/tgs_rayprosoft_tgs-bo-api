using NSubstitute;
using NUnit.Framework.Legacy;
using Tgs.Paging;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class UserTests
{
    [Test]
    public async Task Count_Users_Query()
    {
        var fakeQueries = Substitute.For<ITgsQueries<User>>();

        var sut = new GetUsersQueryService(fakeQueries);

        var paging = new Paging<UserInfo>
        {
            RowCount = 10,
            Page = 0,
        };

        var resellerId = Guid.NewGuid();
        var licenseeId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();

        var query = new GetUsersQuery(
            resellerId,
            licenseeId,
            brandId,
            "",
            "",
            "",
            "",
            paging);

        _ = fakeQueries
            .GetQueries()
            .Returns(new User[]
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Username = "Username",
                    Name = "Name",
                    RoleId = roleId,
                    Role = new Role()
                    {
                        Id = roleId,
                        ResellerId = resellerId,
                        LicenseeId = licenseeId,
                        BrandId = brandId,
                        Name = "RoleName",
                    },
                    DepartmentId = departmentId,
                    Department = new Department()
                    {
                        Id = departmentId,
                        Name = "DepartmentName"
                    },
                    LastLoginIp = "127.0.0.1",
                    LastLoginTime = DateTime.Now.AddDays(-14),
                    Status = UserStatus.Enabled,
                    CreatedOn = DateTime.Now.AddMonths(-1),
                }
            }.AsQueryable());

        var actual = await sut.GetAsync(query);

        ClassicAssert.AreEqual(1, actual);
    }

    [Test]
    public async Task Get_Users_Query()
    {
        var fakeQueries = Substitute.For<ITgsQueries<User>>();

        var sut = new GetUsersQueryService(fakeQueries);

        var paging = new Paging<UserInfo>
        {
            RowCount = 10,
            Page = 0,
        };

        var resellerId = Guid.NewGuid();
        var licenseeId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();

        var query = new GetUsersQuery(
            resellerId,
            licenseeId,
            brandId,
            "",
            "",
            "",
            "",
            paging);

        _ = fakeQueries
            .GetQueries()
            .Returns(new User[]
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Username = "Username",
                    Name = "Name",
                    RoleId = roleId,
                    Role = new Role()
                    {
                        Id = roleId,
                        ResellerId = resellerId,
                        LicenseeId = licenseeId,
                        BrandId = brandId,
                        Name = "RoleName",
                    },
                    DepartmentId = departmentId,
                    Department = new Department()
                    {
                        Id = departmentId,
                        Name = "DepartmentName"
                    },
                    LastLoginIp = "127.0.0.1",
                    LastLoginTime = DateTime.Now.AddDays(-14),
                    Status = UserStatus.Enabled,
                    CreatedOn = DateTime.Now.AddMonths(-1),
                }
            }.AsQueryable());

        var actual = await sut.QueryAsync(query).ToArrayAsync();

        ClassicAssert.AreEqual(1, actual.Length);
    }
}
