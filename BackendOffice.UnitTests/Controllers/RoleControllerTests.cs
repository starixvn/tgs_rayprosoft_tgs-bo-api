using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using TGS.Apis.BackendOffice.Controllers;
using TGS.Apis.BackendOffice.Controllers.Messages.Roles;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Json;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Services;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class RoleControllerTests : BaseTests
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();

        _controller = new(
            _roleQueriesMock.Object,
            _settingQueriesMock.Object,
            _roleCommandsMock.Object,
            _resellerQueriesMock.Object,
            _licenseeQueriesMock.Object,
            _brandQueriesMock.Object,
            _jsonSerializationProviderMock.Object);
        _controller.ControllerContext.HttpContext = HttpContextMock.Object;
    }

    private RoleController _controller = default!;
    private readonly Mock<ITgsQueries<Role>> _roleQueriesMock = new();
    private readonly Mock<ITgsQueries<Setting>> _settingQueriesMock = new();
    private readonly Mock<ITgsCommands<Role>> _roleCommandsMock = new();
    private readonly Mock<ITgsQueries<Reseller>> _resellerQueriesMock = new();
    private readonly Mock<ITgsQueries<Licensee>> _licenseeQueriesMock = new();
    private readonly Mock<ITgsQueries<Brand>> _brandQueriesMock = new();

    private readonly Mock<IJsonSerializationProvider> _jsonSerializationProviderMock =
        new();

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Roles(bool showAll)
    {
        var request = new GetRolesRequest
        {
            Page = 1,
            Rows = 10,
            SortBy = "Name",
            OrderBy = SortOrder.Descending,
            ShowAll = showAll
        };

        var roles = CreateRoleFixture()
            .CreateMany<Role>(105)
            .AsQueryable()
            .BuildMock();

        var resellers = CreateResellerFixture().CreateMany<Reseller>(10).AsQueryable().BuildMock();
        var licensees = CreateLicenseeFixture().CreateMany<Licensee>(10).AsQueryable().BuildMock();
        var brands = CreateBrandFixture().CreateMany<Brand>(10).AsQueryable().BuildMock();

        var (skip, take) = GetPage(request);
        var orderedRoles = roles.OrderByDescending(p => p.Name);

        var expectedRoles = showAll ? orderedRoles.ToArray() : orderedRoles.Skip(skip).Take(take).ToArray();

        _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        _resellerQueriesMock.Setup(x => x.GetQueries())
            .Returns(resellers);

        _licenseeQueriesMock.Setup(x => x.GetQueries())
            .Returns(licensees);

        _brandQueriesMock.Setup(x => x.GetQueries())
            .Returns(brands);

        var response = await _controller.GetListAsync(request);
        response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            response.Items[i].Id.Should().Be(expectedRoles[i].Id);
            response.Items[i].Name.Should().Be(expectedRoles[i].Name);
            response.Items[i].DisplayName.Should().Be(expectedRoles[i].DisplayName);
            response.Items[i].Description.Should().Be(expectedRoles[i].Description);
            response.Items[i].CreatedOn.Should().Be(expectedRoles[i].CreatedOn);
            response.Items[i].UpdatedOn.Should().Be(expectedRoles[i].UpdatedOn);
            response.Items[i].CreatedByUsername.Should().Be(expectedRoles[i].CreatedByUsername);
            response.Items[i].UpdatedByUsername.Should().Be(expectedRoles[i].UpdatedByUsername);
        }

        _roleQueriesMock.Verify(p => p.GetQueries());
    }

    [Test]
    public async Task Can_Get_Role()
    {
        var role = CreateRoleFixture().Create<Role>();

        _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        _settingQueriesMock.Setup(s => s.GetSingleAsync(p => p.Key == "Menus", null))
            .Returns(Task.FromResult(new Fixture().Create<Setting>()));

        var menuSettings = CreateMenuSettingFixture().CreateMany<MenuSetting>(10).ToArray();

        _jsonSerializationProviderMock.SetupSequence(p =>
                p.DeserializeFromString<MenuSetting[]>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(menuSettings)
            .Returns(menuSettings);

        var response = await _controller.GetSingleAsync(role.Id);
        response.Valid.Should().BeTrue();
        response.Role.Id.Should().Be(role.Id);
        response.Role.Name.Should().Be(role.Name);
        response.Role.DisplayName.Should().Be(role.DisplayName);
        response.Role.Description.Should().Be(role.Description);
        response.Role.CreatedByUsername.Should().Be(role.CreatedByUsername);
        response.Role.UpdatedByUsername.Should().Be(role.UpdatedByUsername);
        response.Role.CreatedOn.Should().Be(role.CreatedOn);
        response.Role.UpdatedOn.Should().Be(role.UpdatedOn);

        for (var i = 0; i < response.Role.Menus.Length; i++)
        {
            response.Role.Menus[i].Code.Should().Be(menuSettings[i].Code);
            response.Role.Menus[i].IsDisplay.Should().Be(menuSettings[i].IsDisplay);

            for (var j = 0; j < response.Role.Menus[i].SubMenus.Length; j++)
            {
                var actualMenu = response.Role.Menus[i].SubMenus[j];
                var expectedMenu = menuSettings[i].SubMenus[j];
                actualMenu.Code.Should().Be(expectedMenu.Code);
                actualMenu.IsDisplay.Should().Be(expectedMenu.IsDisplay);
            }
        }
    }

    [Test]
    public async Task Cannot_Get_Role()
    {
        var roleId = Guid.NewGuid();

        _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult<Role>(default!));

        var response = await _controller.GetSingleAsync(roleId);
        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NotFound);
    }

    [Test]
    public async Task Can_Create_Role()
    {
        var request = CreateRoleRequestFixture().Create<CreateRoleRequest>();
        var roles = CreateRoleFixture().CreateMany<Role>(10).AsQueryable().BuildMock();

        _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        _roleQueriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(Task.FromResult(false));

        var response = await _controller.CreateAsync(request);
        response.Valid.Should().BeTrue();

        _roleCommandsMock.Verify(s => s.CreateAsync(It.Is<Role>(p =>
            p.Name == request.Name &&
            p.DisplayName == request.DisplayName &&
            p.Description == request.Description)));
    }

    [Test]
    public async Task Cannot_Create_User_With_Name_Exists()
    {
        var request = CreateRoleRequestFixture().Create<CreateRoleRequest>();
        var roles = CreateRoleFixture().CreateMany<Role>(10).AsQueryable().BuildMock();
        request.Name = "Name";
        request.ResellerId = null;
        request.LicenseeId = null;
        request.BrandId = null;

        roles.First().Name = "Name";
        roles.First().ResellerId = null;
        roles.First().LicenseeId = null;
        roles.First().BrandId = null;

        _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        var response = await _controller.CreateAsync(request);
        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NameExists);
    }

    [Test]
    public async Task Can_Update_Admin_Role()
    {
        var roles = CreateRoleFixture()
            .CreateMany<Role>(10)
            .AsQueryable()
            .BuildMock();
        var firstId = roles.First().Id;
        var index = 0;
        var role = new Role();

        await roles.ForEachAsync(x =>
        {
            if (firstId == x.Id)
            {
                x.IsRoot = true;
                x.RoleType = RoleType.Brand;
            }

            if (2 == index)
            {
                x.IsRoot = true;
                x.RoleType = RoleType.Admin;
                role = x;
            }

            index++;
        });

        var request = UpdateRoleRequestFixture().Create<UpdateRoleRequest>();

        var menuSettings = CreateMenuSettingFixture().CreateMany<MenuSetting>(10).ToArray();

        _jsonSerializationProviderMock.Setup(p =>
                p.DeserializeFromString<MenuSetting[]>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(menuSettings);

        _jsonSerializationProviderMock.Setup(p =>
                p.SerializeToString(It.IsAny<MenuSetting[]>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(role.MenuSetting);

        _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        _roleQueriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(Task.FromResult(false));

        _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        var response = await _controller.UpdateAsync(role.Id, request);

        response.Valid.Should().BeTrue();

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));

        _roleCommandsMock.Verify(p => p.UpdateAsync(It.Is<Role>(r => r.MenuSetting == role.MenuSetting)),
            Times.Once);
    }

    [Test]
    public async Task Can_Update_Reseller_Root_Role()
    {
        var roles = CreateRoleFixture()
            .CreateMany<Role>(10)
            .AsQueryable()
            .BuildMock();
        var firstId = roles.First().Id;
        var index = 0;
        var role = new Role();

        await roles.ForEachAsync(x =>
        {
            if (firstId == x.Id)
            {
                x.IsRoot = true;
                x.RoleType = RoleType.Brand;
            }

            if (2 == index)
            {
                x.IsRoot = true;
                x.RoleType = RoleType.Reseller;
                role = x;
            }

            if (3 == index)
            {
                x.IsRoot = true;
                x.RoleType = RoleType.Licensee;
                x.ResellerId = role.ResellerId;
            }

            index++;
        });

        var request = UpdateRoleRequestFixture().Create<UpdateRoleRequest>();

        var menuSettings = CreateMenuSettingFixture().CreateMany<MenuSetting>(10).ToArray();

        _jsonSerializationProviderMock.Setup(p =>
                p.DeserializeFromString<MenuSetting[]>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(menuSettings);

        _jsonSerializationProviderMock.Setup(p =>
                p.SerializeToString(It.IsAny<MenuSetting[]>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(role.MenuSetting);

        _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        _roleQueriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(Task.FromResult(false));

        _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        var response = await _controller.UpdateAsync(role.Id, request);

        response.Valid.Should().BeTrue();

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));

        _roleCommandsMock.Verify(p => p.BatchUpdateAsync(It.Is<Role[]>(r =>
            r.Length == 2)));
    }

    [Test]
    public async Task Can_Update_Reseller_Not_Root_Role()
    {
        var roles = CreateRoleFixture()
            .CreateMany<Role>(10)
            .AsQueryable()
            .BuildMock();
        var index = 0;
        var role = new Role();
        var resellerRootRole = new Role();

        await roles.ForEachAsync(x =>
        {
            if (2 == index)
            {
                x.IsRoot = true;
                x.RoleType = RoleType.Reseller;
                resellerRootRole = x;
            }

            if (3 == index)
            {
                x.IsRoot = false;
                x.RoleType = RoleType.Reseller;
                x.ResellerId = resellerRootRole.ResellerId;
                role = x;
            }

            if (4 == index)
            {
                x.IsRoot = true;
                x.RoleType = RoleType.Licensee;
                x.ResellerId = resellerRootRole.ResellerId;
            }

            if (5 == index)
            {
                x.IsRoot = true;
                x.RoleType = RoleType.Brand;
                x.ResellerId = resellerRootRole.ResellerId;
            }


            index++;
        });

        var request = UpdateRoleRequestFixture().Create<UpdateRoleRequest>();

        var menuSettings = CreateMenuSettingFixture().CreateMany<MenuSetting>(10).ToArray();

        _jsonSerializationProviderMock.Setup(p =>
                p.DeserializeFromString<MenuSetting[]>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(menuSettings);

        _jsonSerializationProviderMock.Setup(p =>
                p.SerializeToString(It.IsAny<MenuSetting[]>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(role.MenuSetting);

        _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        _roleQueriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(Task.FromResult(false));

        _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        var response = await _controller.UpdateAsync(role.Id, request);

        response.Valid.Should().BeTrue();

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));

        _roleCommandsMock.Verify(p => p.UpdateAsync(It.Is<Role>(r =>
                r.MenuSetting == role.MenuSetting)),
            Times.Once);

        _roleCommandsMock.Verify(p => p.BatchUpdateAsync(It.Is<Role[]>(r =>
                r.Length == 1)),
            Times.Never);
    }

    [Test]
    public async Task Can_Update_Licensee_Not_Root_Role()
    {
        var roles = CreateRoleFixture()
            .CreateMany<Role>(10)
            .AsQueryable()
            .BuildMock();
        var firstId = roles.First().Id;
        var index = 0;
        var role = new Role();
        var licenseeRootRole = new Role();


        await roles.ForEachAsync(x =>
        {
            if (firstId == x.Id)
            {
                x.IsRoot = true;
                x.RoleType = RoleType.Brand;
            }

            if (2 == index)
            {
                x.IsRoot = true;
                x.RoleType = RoleType.Licensee;
                licenseeRootRole = x;
            }

            if (3 == index)
            {
                x.IsRoot = false;
                x.RoleType = RoleType.Licensee;
                x.LicenseeId = licenseeRootRole.Id;
                role = x;
            }

            if (4 == index)
            {
                x.IsRoot = true;
                x.RoleType = RoleType.Licensee;
            }

            index++;
        });

        var request = UpdateRoleRequestFixture().Create<UpdateRoleRequest>();

        var menuSettings = CreateMenuSettingFixture().CreateMany<MenuSetting>(10).ToArray();

        _jsonSerializationProviderMock.Setup(p =>
                p.DeserializeFromString<MenuSetting[]>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(menuSettings);

        _jsonSerializationProviderMock.Setup(p =>
                p.SerializeToString(It.IsAny<MenuSetting[]>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(role.MenuSetting);

        _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        _roleQueriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(Task.FromResult(false));

        _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        var response = await _controller.UpdateAsync(role.Id, request);

        response.Valid.Should().BeTrue();

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));

        _roleCommandsMock.Verify(p => p.UpdateAsync(It.Is<Role>(r =>
                r.MenuSetting == role.MenuSetting)),
            Times.Once);

        _roleCommandsMock.Verify(p => p.BatchUpdateAsync(It.Is<Role[]>(r =>
                r.Length == 1)),
            Times.Never);
    }

    [Test]
    public async Task Can_Update_Licensee_Root_Role()
    {
        var roles = CreateRoleFixture()
            .CreateMany<Role>(10)
            .AsQueryable()
            .BuildMock();
        var firstId = roles.First().Id;
        var index = 0;
        var role = new Role();

        await roles.ForEachAsync(x =>
        {
            if (firstId == x.Id)
            {
                x.IsRoot = true;
                x.RoleType = RoleType.Brand;
            }

            if (2 == index)
            {
                x.IsRoot = true;
                x.RoleType = RoleType.Licensee;
                role = x;
            }

            if (3 == index)
            {
                x.IsRoot = false;
                x.RoleType = RoleType.Licensee;
                x.LicenseeId = role.LicenseeId;
            }

            if (4 == index)
            {
                x.IsRoot = true;
                x.RoleType = RoleType.Brand;
                x.LicenseeId = role.LicenseeId;
            }

            index++;
        });

        var request = UpdateRoleRequestFixture().Create<UpdateRoleRequest>();

        var menuSettings = CreateMenuSettingFixture().CreateMany<MenuSetting>(10).ToArray();

        _jsonSerializationProviderMock.Setup(p =>
                p.DeserializeFromString<MenuSetting[]>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(menuSettings);

        _jsonSerializationProviderMock.Setup(p =>
                p.SerializeToString(It.IsAny<MenuSetting[]>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(role.MenuSetting);

        _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        _roleQueriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(Task.FromResult(false));

        _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        var response = await _controller.UpdateAsync(role.Id, request);

        response.Valid.Should().BeTrue();

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));

        _roleCommandsMock.Verify(p => p.BatchUpdateAsync(It.Is<Role[]>(r =>
            r.Length == 3)));
    }

    [Test]
    public async Task Can_Update_Brand_Root_Role()
    {
        var roles = CreateRoleFixture()
            .CreateMany<Role>(10)
            .AsQueryable()
            .BuildMock();
        var index = 0;
        var role = new Role();

        await roles.ForEachAsync(x =>
        {
            if (0 == index)
            {
                x.IsRoot = true;
                x.RoleType = RoleType.Brand;
                role = x;
            }

            if (2 == index)
            {
                x.IsRoot = true;
                x.RoleType = RoleType.Licensee;
                x.LicenseeId = role.LicenseeId;
            }

            if (3 == index)
            {
                x.IsRoot = false;
                x.RoleType = RoleType.Licensee;
                x.LicenseeId = role.LicenseeId;
            }

            if (4 == index)
            {
                x.IsRoot = false;
                x.RoleType = RoleType.Brand;
                x.BrandId = role.BrandId;
            }

            index++;
        });

        var request = UpdateRoleRequestFixture().Create<UpdateRoleRequest>();

        var menuSettings = CreateMenuSettingFixture().CreateMany<MenuSetting>(10).ToArray();

        _jsonSerializationProviderMock.Setup(p =>
                p.DeserializeFromString<MenuSetting[]>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(menuSettings);

        _jsonSerializationProviderMock.Setup(p =>
                p.SerializeToString(It.IsAny<MenuSetting[]>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(role.MenuSetting);

        _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        _roleQueriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(Task.FromResult(false));

        _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        var response = await _controller.UpdateAsync(role.Id, request);

        response.Valid.Should().BeTrue();

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));

        _roleCommandsMock.Verify(p => p.BatchUpdateAsync(It.Is<Role[]>(r =>
            r.Length == 2)));
    }

    [Test]
    public async Task Can_Update_Brand_Not_Root_Role()
    {
        var roles = CreateRoleFixture()
            .CreateMany<Role>(10)
            .AsQueryable()
            .BuildMock();
        var index = 0;
        var role = new Role();
        var brandRootRole = new Role();

        await roles.ForEachAsync(x =>
        {
            if (0 == index)
            {
                x.IsRoot = true;
                x.RoleType = RoleType.Brand;
                brandRootRole = x;
            }

            if (2 == index)
            {
                x.IsRoot = false;
                x.RoleType = RoleType.Brand;
                x.BrandId = brandRootRole.BrandId;
                role = x;
            }

            if (3 == index)
            {
                x.IsRoot = false;
                x.RoleType = RoleType.Licensee;
                x.LicenseeId = role.LicenseeId;
            }

            if (4 == index)
            {
                x.IsRoot = false;
                x.RoleType = RoleType.Brand;
                x.BrandId = role.BrandId;
            }

            if (5 == index)
            {
                x.IsRoot = true;
                x.RoleType = RoleType.Licensee;
                x.LicenseeId = role.LicenseeId;
            }


            index++;
        });

        var request = UpdateRoleRequestFixture().Create<UpdateRoleRequest>();

        var menuSettings = CreateMenuSettingFixture().CreateMany<MenuSetting>(10).ToArray();

        _jsonSerializationProviderMock.Setup(p =>
                p.DeserializeFromString<MenuSetting[]>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(menuSettings);

        _jsonSerializationProviderMock.Setup(p =>
                p.SerializeToString(It.IsAny<MenuSetting[]>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(role.MenuSetting);

        _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        _roleQueriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(Task.FromResult(false));

        _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        var response = await _controller.UpdateAsync(role.Id, request);

        response.Valid.Should().BeTrue();

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));

        _roleCommandsMock.Verify(p => p.UpdateAsync(It.Is<Role>(r =>
                r.MenuSetting == role.MenuSetting)),
            Times.Once);

        _roleCommandsMock.Verify(p => p.BatchUpdateAsync(It.Is<Role[]>(r =>
                r.Length == 2)),
            Times.Never);
    }

    [Test]
    public async Task Cannot_Update_Role_Not_Found()
    {
        var role = CreateRoleFixture().Create<Role>();
        var request = UpdateRoleRequestFixture().Create<UpdateRoleRequest>();

        _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult<Role>(default!));

        _roleQueriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(Task.FromResult(true));


        var response = await _controller.UpdateAsync(role.Id, request);

        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NotFound);

        _roleCommandsMock.Verify(p => p.UpdateAsync(It.Is<Role>(r =>
                r.Name == role.Name &&
                r.DisplayName == role.DisplayName &&
                r.Description == role.Description)),
            Times.Never);
    }

    [Test]
    public async Task Cannot_Update_Role_With_Name_Exists()
    {
        var role = CreateRoleFixture().Create<Role>();
        var request = UpdateRoleRequestFixture().Create<UpdateRoleRequest>();
        var roles = CreateRoleFixture().CreateMany<Role>(10).AsQueryable().BuildMock();
        request.Name = "Name";
        role.ResellerId = null;
        role.LicenseeId = null;
        role.BrandId = null;

        var firstRole = roles.First();
        firstRole.Name = "Name";
        firstRole.ResellerId = null;
        firstRole.LicenseeId = null;
        firstRole.BrandId = null;

        _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        _roleQueriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(Task.FromResult(true));

        var response = await _controller.UpdateAsync(role.Id, request);

        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NameExists);

        _roleCommandsMock.Verify(p => p.UpdateAsync(It.Is<Role>(r =>
                r.Name == role.Name &&
                r.DisplayName == role.DisplayName &&
                r.Description == role.Description)),
            Times.Never);
    }


    [Test]
    public async Task Can_Copy_Role()
    {
        var role = CreateRoleFixture().Create<Role>();
        var request = new Fixture().Create<CopyRoleRequest>();
        role.IsRoot = false;
        var roles = CreateRoleFixture().CreateMany<Role>(10).AsQueryable().BuildMock();

        _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        var response = await _controller.CopyAsync(role.Id, request);

        response.Valid.Should().BeTrue();

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));

        _roleCommandsMock.Verify(p => p.CreateAsync(It.Is<Role>(r =>
            r.Name == request.Name &&
            r.DisplayName == role.DisplayName &&
            r.Description == role.Description)));
    }

    [Test]
    public async Task Cannot_Copy_Role_Not_Found()
    {
        var role = CreateRoleFixture().Create<Role>();
        var request = new Fixture().Create<CopyRoleRequest>();

        _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult<Role>(default!));

        _roleQueriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(Task.FromResult(true));

        var response = await _controller.CopyAsync(role.Id, request);

        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NotFound);

        _roleCommandsMock.Verify(p => p.UpdateAsync(It.Is<Role>(r =>
                r.Name == role.Name &&
                r.DisplayName == role.DisplayName &&
                r.Description == role.Description)),
            Times.Never);
    }

    [Test]
    public async Task Cannot_Copy_Role_With_Name_Exists()
    {
        var role = CreateRoleFixture().Create<Role>();
        var request = UpdateRoleRequestFixture().Create<CopyRoleRequest>();
        role.IsRoot = false;
        var roles = CreateRoleFixture().CreateMany<Role>(10).AsQueryable().BuildMock();
        request.Name = "Name";
        role.ResellerId = null;
        role.LicenseeId = null;
        role.BrandId = null;

        var firstRole = roles.First();
        firstRole.Name = "Name";
        firstRole.ResellerId = null;
        firstRole.LicenseeId = null;
        firstRole.BrandId = null;

        _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        var response = await _controller.CopyAsync(role.Id, request);

        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NameExists);

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));

        _roleCommandsMock.Verify(p => p.UpdateAsync(It.Is<Role>(r =>
                r.Name == role.Name &&
                r.DisplayName == role.DisplayName &&
                r.Description == role.Description)),
            Times.Never);
    }

    [Test]
    public async Task Can_Delete_Role()
    {
        var role = CreateRoleFixture().Create<Role>();

        _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));
        role.Users = new List<User>();
        role.IsRoot = false;

        var response = await _controller.DeleteAsync(role.Id);

        response.Valid.Should().BeTrue();

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));

        _roleCommandsMock.Verify(p => p.DeleteAsync(It.Is<Role>(r =>
            r.Name == role.Name &&
            r.DisplayName == role.DisplayName &&
            r.Description == role.Description)));
    }

    [Test]
    public async Task Can_Not_Delete_Role_When_bind_With_Users()
    {
        var role = CreateRoleFixture().Create<Role>();

        role.Users = new List<User>
        {
            new()
        };

        _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        var response = await _controller.DeleteAsync(role.Id);

        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Apis.BackendOffice.Resources.Role.CannotDeleteRole);

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));
    }

    [Test]
    public async Task Can_Not_Delete_Role()
    {
        var role = CreateRoleFixture().Create<Role>();

        _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        var response = await _controller.DeleteAsync(role.Id);

        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Apis.BackendOffice.Resources.Role.CannotDeleteRootRole);

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));
    }

    [Test]
    public async Task Cannot_Delete_Role_Not_Found()
    {
        var role = CreateRoleFixture().Create<Role>();

        _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult<Role>(default!));

        var response = await _controller.DeleteAsync(role.Id);

        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NotFound);

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));

        _roleCommandsMock.Verify(p => p.DeleteAsync(It.Is<Role>(r =>
                r.Name == role.Name &&
                r.DisplayName == role.DisplayName &&
                r.Description == role.Description)),
            Times.Never);
    }

    [Test]
    public async Task Cannot_Delete_Role_With_User_Used()
    {
        var role = CreateRoleFixture().Create<Role>();

        role.Users = new List<User>
        {
            new()
        };

        _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        var response = await _controller.DeleteAsync(role.Id);

        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Apis.BackendOffice.Resources.Role.CannotDeleteRole);

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));

        _roleCommandsMock.Verify(p => p.DeleteAsync(It.Is<Role>(r =>
                r.Name == role.Name &&
                r.DisplayName == role.DisplayName &&
                r.Description == role.Description)),
            Times.Never);
    }

    [Test]
    public async Task Can_Get_Menus()
    {
        var role = CreateRoleFixture().Create<Role>();

        role.Users = new List<User>
        {
            new()
        };

        var request = new GetMenuRequest
        {
            RoleType = RoleType.Admin
        };

        _settingQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Setting, bool>>>(),
                It.IsAny<Func<IQueryable<Setting>, IQueryable<Setting>>>()))
            .Returns(Task.FromResult(new Setting
            {
                Value = "{}"
            }));

        var menuSettings = CreateMenuSettingFixture().CreateMany<MenuSetting>(10).ToArray();

        _jsonSerializationProviderMock.Setup(p =>
                p.DeserializeFromString<MenuSetting[]>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(menuSettings);

        var response = await _controller.GetMenuAsync(request);
        response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Menus.Length; i++)
        {
            response.Menus[i].Code.Should().Be(menuSettings[i].Code);
            response.Menus[i].IsDisplay.Should().Be(menuSettings[i].IsDisplay);

            for (var j = 0; j < response.Menus[i].SubMenus.Length; j++)
            {
                var actualMenu = response.Menus[i].SubMenus[j];
                var expectedMenu = menuSettings[i].SubMenus[j];
                actualMenu.Code.Should().Be(expectedMenu.Code);
                actualMenu.IsDisplay.Should().Be(expectedMenu.IsDisplay);
            }
        }
    }

    [Test]
    public async Task Cannot_Get_Menus()
    {
        var role = CreateRoleFixture().Create<Role>();

        var request = new GetMenuRequest
        {
            RoleType = RoleType.Admin
        };

        role.Users = new List<User>
        {
            new()
        };

        _settingQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Setting, bool>>>(),
                It.IsAny<Func<IQueryable<Setting>, IQueryable<Setting>>>()))
            .Returns(Task.FromResult<Setting>(default!));

        var response = await _controller.GetMenuAsync(request);
        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Apis.BackendOffice.Resources.Role.MenuNotFound);
    }

    [Test]
    public async Task Cannot_Get_Menus_When_Is_Licensee_Or_Brand_Role()
    {
        var role = CreateRoleFixture().Create<Role>();

        var request = new GetMenuRequest
        {
            RoleType = RoleType.Licensee
        };

        role.Users = new List<User>
        {
            new()
        };

        _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult<Role>(default!));

        var response = await _controller.GetMenuAsync(request);
        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Apis.BackendOffice.Resources.Role.MenuNotFound);
    }

    private Fixture CreateRoleFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<Role>(u => u
            .With(s => s.Users,
                new User[]
                {
                })
            .With(s => s.MenuSetting));

        return fixture;
    }

    private Fixture CreateMenuSettingFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<MenuSetting>(u => u
            .With(s => s.SubMenus,
                new MenuSetting[]
                {
                }));


        fixture.Customize<MenuSetting>(u => u
            .With(s => s.SubMenus, fixture.CreateMany<MenuSetting>(5).ToArray()));

        return fixture;
    }

    private Fixture CreateRoleRequestFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<CreateRoleRequest>(u => u
            .With(s => s.Menus,
                new MenuSetting[]
                {
                }));

        return fixture;
    }

    private Fixture UpdateRoleRequestFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<UpdateRoleRequest>(u => u
            .With(s => s.Menus,
                new MenuSetting[]
                {
                }));

        return fixture;
    }

    private Fixture CreateResellerFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<Reseller>(u => u
            .With(s => s.GameProviders,
                new ResellerGameProvider[]
                {
                })
            .With(s => s.Languages,
                new ResellerLanguage[]
                {
                })
            .With(s => s.Currencies,
                new ResellerCurrency[]
                {
                })
            .With(s => s.BetLimits,
                new ResellerBetLimit[]
                {
                })
            .With(s => s.Licensees,
                new Licensee[]
                {
                })
            .With(s => s.Departments,
                new Department[]
                {
                }));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        return fixture;
    }

    private Fixture CreateLicenseeFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<Licensee>(u => u
            .With(s => s.Reseller, new Reseller())
            .With(s => s.Brands,
                new Brand[]
                {
                })
            .With(s => s.GameProviders,
                new LicenseeGameProvider[]
                {
                })
            .With(s => s.Languages,
                new LicenseeLanguage[]
                {
                })
            .With(s => s.Currencies,
                new LicenseeCurrency[]
                {
                })
            .With(s => s.BetLimits,
                new LicenseeBetLimit[]
                {
                })
            .With(s => s.Departments,
                new Department[]
                {
                }));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        return fixture;
    }

    private Fixture CreateBrandFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<Brand>(u => u
            .With(s => s.Licensee, new Licensee())
            .With(s => s.GameProviders,
                new BrandGameProvider[]
                {
                })
            .With(s => s.Languages,
                new BrandLanguage[]
                {
                })
            .With(s => s.Currencies,
                new BrandCurrency[]
                {
                })
            .With(s => s.Players,
                new Player[]
                {
                })
            .With(s => s.Games,
                new BrandGame[]
                {
                })
            .With(s => s.BetLimits,
                new BrandBetLimit[]
                {
                })
            .With(s => s.BrandGroups,
                new BrandGroup[]
                {
                })
            .With(s => s.PlayerLoginRecords,
                new PlayerLoginRecord[]
                {
                })
            .With(s => s.Announcements,
                new Announcement[]
                {
                })
            .With(s => s.StationLetters,
                new StationLetter[]
                {
                })
            .With(s => s.VipLevels,
                new VipLevel[]
                {
                })
            .With(s => s.Departments,
                new Department[]
                {
                })
            .With(s => s.Domains,
                new BrandDomain[]
                {
                }));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        return fixture;
    }
}