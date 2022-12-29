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
    private readonly Mock<ITgsQueries<Brand>> _brandQueriesMock = new();

    private readonly Mock<IJsonSerializationProvider> _jsonSerializationProviderMock =
        new();

    private readonly Mock<ITgsQueries<Licensee>> _licenseeQueriesMock = new();

    private readonly Mock<ITgsQueries<Reseller>> _resellerQueriesMock = new();

    private readonly Mock<ITgsCommands<Role>> _roleCommandsMock = new();

    private readonly Mock<ITgsQueries<Role>> _roleQueriesMock = new();

    private readonly Mock<ITgsQueries<Setting>> _settingQueriesMock = new();

    private RoleController _controller = default!;

    [Test]
    public async Task Can_Copy_Role()
    {
        var role = CreateRoleFixture().Create<Role>();
        var request = new Fixture().Create<CopyRoleRequest>();
        role.IsRoot = false;
        var roles = CreateRoleFixture().CreateMany<Role>(10).AsQueryable().BuildMock();

        _ = _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        _ = _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        var response = await _controller.CopyAsync(role.Id, request);

        _ = response.Valid.Should().BeTrue();

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));

        _roleCommandsMock.Verify(p => p.CreateAsync(It.Is<Role>(r =>
            r.Name == request.Name &&
            r.DisplayName == role.DisplayName &&
            r.Description == role.Description)));
    }

    [Test]
    public async Task Can_Create_Role()
    {
        var request = CreateRoleRequestFixture().Create<CreateRoleRequest>();
        var roles = CreateRoleFixture().CreateMany<Role>(10).AsQueryable().BuildMock();

        _ = _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        _ = _roleQueriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(Task.FromResult(false));

        var response = await _controller.CreateAsync(request);
        _ = response.Valid.Should().BeTrue();

        _roleCommandsMock.Verify(s => s.CreateAsync(It.Is<Role>(p =>
            p.Name == request.Name &&
            p.DisplayName == request.DisplayName &&
            p.Description == request.Description)));
    }

    [Test]
    public async Task Can_Delete_Role()
    {
        var role = CreateRoleFixture().Create<Role>();

        _ = _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));
        role.Users = new List<User>();
        role.IsRoot = false;

        var response = await _controller.DeleteAsync(role.Id);

        _ = response.Valid.Should().BeTrue();

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));

        _roleCommandsMock.Verify(p => p.DeleteAsync(It.Is<Role>(r =>
            r.Name == role.Name &&
            r.DisplayName == role.DisplayName &&
            r.Description == role.Description)));
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

        _ = _settingQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Setting, bool>>>(),
                It.IsAny<Func<IQueryable<Setting>, IQueryable<Setting>>>()))
            .Returns(Task.FromResult(new Setting
            {
                Value = "{}"
            }));

        var menuSettings = CreateMenuSettingFixture().CreateMany<MenuSetting>(10).ToArray();

        _ = _jsonSerializationProviderMock.Setup(p =>
                p.DeserializeFromString<MenuSetting[]>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(menuSettings);

        var response = await _controller.GetMenuAsync(request);
        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Menus.Length; i++)
        {
            _ = response.Menus[i].Code.Should().Be(menuSettings[i].Code);
            _ = response.Menus[i].IsDisplay.Should().Be(menuSettings[i].IsDisplay);

            for (var j = 0; j < response.Menus[i].SubMenus.Length; j++)
            {
                var actualMenu = response.Menus[i].SubMenus[j];
                var expectedMenu = menuSettings[i].SubMenus[j];
                _ = actualMenu.Code.Should().Be(expectedMenu.Code);
                _ = actualMenu.IsDisplay.Should().Be(expectedMenu.IsDisplay);
            }
        }
    }

    [Test]
    public async Task Can_Get_Role()
    {
        var role = CreateRoleFixture().Create<Role>();

        _ = _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        _ = _settingQueriesMock.Setup(s => s.GetSingleAsync(p => p.Key == "Menus", null))
            .Returns(Task.FromResult(new Fixture().Create<Setting>()));

        var menuSettings = CreateMenuSettingFixture().CreateMany<MenuSetting>(10).ToArray();

        _ = _jsonSerializationProviderMock.SetupSequence(p =>
                p.DeserializeFromString<MenuSetting[]>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(menuSettings)
            .Returns(menuSettings);

        var response = await _controller.GetSingleAsync(role.Id);
        _ = response.Valid.Should().BeTrue();
        _ = response.Role.Id.Should().Be(role.Id);
        _ = response.Role.Name.Should().Be(role.Name);
        _ = response.Role.DisplayName.Should().Be(role.DisplayName);
        _ = response.Role.Description.Should().Be(role.Description);
        _ = response.Role.CreatedByUsername.Should().Be(role.CreatedByUsername);
        _ = response.Role.UpdatedByUsername.Should().Be(role.UpdatedByUsername);
        _ = response.Role.CreatedOn.Should().Be(role.CreatedOn);
        _ = response.Role.UpdatedOn.Should().Be(role.UpdatedOn);

        for (var i = 0; i < response.Role.Menus.Length; i++)
        {
            _ = response.Role.Menus[i].Code.Should().Be(menuSettings[i].Code);
            _ = response.Role.Menus[i].IsDisplay.Should().Be(menuSettings[i].IsDisplay);

            for (var j = 0; j < response.Role.Menus[i].SubMenus.Length; j++)
            {
                var actualMenu = response.Role.Menus[i].SubMenus[j];
                var expectedMenu = menuSettings[i].SubMenus[j];
                _ = actualMenu.Code.Should().Be(expectedMenu.Code);
                _ = actualMenu.IsDisplay.Should().Be(expectedMenu.IsDisplay);
            }
        }
    }

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

        _ = _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        _ = _resellerQueriesMock.Setup(x => x.GetQueries())
            .Returns(resellers);

        _ = _licenseeQueriesMock.Setup(x => x.GetQueries())
            .Returns(licensees);

        _ = _brandQueriesMock.Setup(x => x.GetQueries())
            .Returns(brands);

        var response = await _controller.GetListAsync(request);
        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            _ = response.Items[i].Id.Should().Be(expectedRoles[i].Id);
            _ = response.Items[i].Name.Should().Be(expectedRoles[i].Name);
            _ = response.Items[i].DisplayName.Should().Be(expectedRoles[i].DisplayName);
            _ = response.Items[i].Description.Should().Be(expectedRoles[i].Description);
            _ = response.Items[i].CreatedOn.Should().Be(expectedRoles[i].CreatedOn);
            _ = response.Items[i].UpdatedOn.Should().Be(expectedRoles[i].UpdatedOn);
            _ = response.Items[i].CreatedByUsername.Should().Be(expectedRoles[i].CreatedByUsername);
            _ = response.Items[i].UpdatedByUsername.Should().Be(expectedRoles[i].UpdatedByUsername);
        }

        _roleQueriesMock.Verify(p => p.GetQueries());
    }

    [Test]
    public async Task Can_Not_Delete_Role()
    {
        var role = CreateRoleFixture().Create<Role>();

        _ = _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        var response = await _controller.DeleteAsync(role.Id);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.CannotDeleteRootRole);

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));
    }

    [Test]
    public async Task Can_Not_Delete_Role_When_bind_With_Users()
    {
        var role = CreateRoleFixture().Create<Role>();

        role.Users = new List<User>
        {
            new()
        };

        _ = _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        var response = await _controller.DeleteAsync(role.Id);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.CannotDeleteRole);

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));
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

        _ = _jsonSerializationProviderMock.Setup(p =>
                p.DeserializeFromString<MenuSetting[]>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(menuSettings);

        _ = _jsonSerializationProviderMock.Setup(p =>
                p.SerializeToString(It.IsAny<MenuSetting[]>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(role.MenuSetting);

        _ = _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        _ = _roleQueriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(Task.FromResult(false));

        _ = _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        var response = await _controller.UpdateAsync(role.Id, request);

        _ = response.Valid.Should().BeTrue();

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));

        _roleCommandsMock.Verify(p => p.UpdateAsync(It.Is<Role>(r => r.MenuSetting == role.MenuSetting)),
            Times.Once);
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

        _ = _jsonSerializationProviderMock.Setup(p =>
                p.DeserializeFromString<MenuSetting[]>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(menuSettings);

        _ = _jsonSerializationProviderMock.Setup(p =>
                p.SerializeToString(It.IsAny<MenuSetting[]>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(role.MenuSetting);

        _ = _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        _ = _roleQueriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(Task.FromResult(false));

        _ = _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        var response = await _controller.UpdateAsync(role.Id, request);

        _ = response.Valid.Should().BeTrue();

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

        _ = _jsonSerializationProviderMock.Setup(p =>
                p.DeserializeFromString<MenuSetting[]>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(menuSettings);

        _ = _jsonSerializationProviderMock.Setup(p =>
                p.SerializeToString(It.IsAny<MenuSetting[]>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(role.MenuSetting);

        _ = _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        _ = _roleQueriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(Task.FromResult(false));

        _ = _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        var response = await _controller.UpdateAsync(role.Id, request);

        _ = response.Valid.Should().BeTrue();

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));

        _roleCommandsMock.Verify(p => p.BatchUpdateAsync(It.Is<Role[]>(r =>
            r.Length == 2)));
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

        _ = _jsonSerializationProviderMock.Setup(p =>
                p.DeserializeFromString<MenuSetting[]>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(menuSettings);

        _ = _jsonSerializationProviderMock.Setup(p =>
                p.SerializeToString(It.IsAny<MenuSetting[]>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(role.MenuSetting);

        _ = _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        _ = _roleQueriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(Task.FromResult(false));

        _ = _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        var response = await _controller.UpdateAsync(role.Id, request);

        _ = response.Valid.Should().BeTrue();

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

        _ = _jsonSerializationProviderMock.Setup(p =>
                p.DeserializeFromString<MenuSetting[]>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(menuSettings);

        _ = _jsonSerializationProviderMock.Setup(p =>
                p.SerializeToString(It.IsAny<MenuSetting[]>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(role.MenuSetting);

        _ = _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        _ = _roleQueriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(Task.FromResult(false));

        _ = _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        var response = await _controller.UpdateAsync(role.Id, request);

        _ = response.Valid.Should().BeTrue();

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));

        _roleCommandsMock.Verify(p => p.BatchUpdateAsync(It.Is<Role[]>(r =>
            r.Length == 3)));
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

        _ = _jsonSerializationProviderMock.Setup(p =>
                p.DeserializeFromString<MenuSetting[]>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(menuSettings);

        _ = _jsonSerializationProviderMock.Setup(p =>
                p.SerializeToString(It.IsAny<MenuSetting[]>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(role.MenuSetting);

        _ = _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        _ = _roleQueriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(Task.FromResult(false));

        _ = _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        var response = await _controller.UpdateAsync(role.Id, request);

        _ = response.Valid.Should().BeTrue();

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

        _ = _jsonSerializationProviderMock.Setup(p =>
                p.DeserializeFromString<MenuSetting[]>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(menuSettings);

        _ = _jsonSerializationProviderMock.Setup(p =>
                p.SerializeToString(It.IsAny<MenuSetting[]>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(role.MenuSetting);

        _ = _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        _ = _roleQueriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(Task.FromResult(false));

        _ = _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        var response = await _controller.UpdateAsync(role.Id, request);

        _ = response.Valid.Should().BeTrue();

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));

        _roleCommandsMock.Verify(p => p.BatchUpdateAsync(It.Is<Role[]>(r =>
            r.Length == 2)));
    }

    [Test]
    public async Task Cannot_Copy_Role_Not_Found()
    {
        var role = CreateRoleFixture().Create<Role>();
        var request = new Fixture().Create<CopyRoleRequest>();

        _ = _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult<Role>(default!));

        _ = _roleQueriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(Task.FromResult(true));

        var response = await _controller.CopyAsync(role.Id, request);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NotFound);

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

        _ = _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        _ = _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        var response = await _controller.CopyAsync(role.Id, request);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NameExists);

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));

        _roleCommandsMock.Verify(p => p.UpdateAsync(It.Is<Role>(r =>
                r.Name == role.Name &&
                r.DisplayName == role.DisplayName &&
                r.Description == role.Description)),
            Times.Never);
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

        _ = _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        var response = await _controller.CreateAsync(request);
        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NameExists);
    }

    [Test]
    public async Task Cannot_Delete_Role_Not_Found()
    {
        var role = CreateRoleFixture().Create<Role>();

        _ = _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult<Role>(default!));

        var response = await _controller.DeleteAsync(role.Id);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NotFound);

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

        _ = _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        var response = await _controller.DeleteAsync(role.Id);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.CannotDeleteRole);

        _roleQueriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()));

        _roleCommandsMock.Verify(p => p.DeleteAsync(It.Is<Role>(r =>
                r.Name == role.Name &&
                r.DisplayName == role.DisplayName &&
                r.Description == role.Description)),
            Times.Never);
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

        _ = _settingQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Setting, bool>>>(),
                It.IsAny<Func<IQueryable<Setting>, IQueryable<Setting>>>()))
            .Returns(Task.FromResult<Setting>(default!));

        var response = await _controller.GetMenuAsync(request);
        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.MenuNotFound);
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

        _ = _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult<Role>(default!));

        var response = await _controller.GetMenuAsync(request);
        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.MenuNotFound);
    }

    [Test]
    public async Task Cannot_Get_Role()
    {
        var roleId = Guid.NewGuid();

        _ = _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult<Role>(default!));

        var response = await _controller.GetSingleAsync(roleId);
        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NotFound);
    }

    [Test]
    public async Task Cannot_Update_Role_Not_Found()
    {
        var role = CreateRoleFixture().Create<Role>();
        var request = UpdateRoleRequestFixture().Create<UpdateRoleRequest>();

        _ = _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult<Role>(default!));

        _ = _roleQueriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(Task.FromResult(true));

        var response = await _controller.UpdateAsync(role.Id, request);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NotFound);

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

        _ = _roleQueriesMock.Setup(x => x.GetQueries())
            .Returns(roles);

        _ = _roleQueriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>()))
            .Returns(Task.FromResult(role));

        _ = _roleQueriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns(Task.FromResult(true));

        var response = await _controller.UpdateAsync(role.Id, request);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NameExists);

        _roleCommandsMock.Verify(p => p.UpdateAsync(It.Is<Role>(r =>
                r.Name == role.Name &&
                r.DisplayName == role.DisplayName &&
                r.Description == role.Description)),
            Times.Never);
    }

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

    private static Fixture CreateBrandFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<Brand>(u => u
            .With(s => s.Licensee, new Licensee())
            .With(s => s.GameProviders,
                Array.Empty<BrandGameProvider>())
            .With(s => s.Languages,
                Array.Empty<BrandLanguage>())
            .With(s => s.Currencies,
                Array.Empty<BrandCurrency>())
            .With(s => s.Players,
                Array.Empty<Player>())
            .With(s => s.Games,
                Array.Empty<BrandGame>())
            .With(s => s.BetLimits,
                Array.Empty<BrandBetLimit>())
            .With(s => s.BrandGroups,
                Array.Empty<BrandGroup>())
            .With(s => s.PlayerLoginRecords,
                Array.Empty<PlayerLoginRecord>())
            .With(s => s.Announcements,
                Array.Empty<Announcement>())
            .With(s => s.StationLetters,
                Array.Empty<StationLetter>())
            .With(s => s.VipLevels,
                Array.Empty<VipLevel>())
            .With(s => s.Departments,
                Array.Empty<Department>())
            .With(s => s.Domains,
                Array.Empty<BrandDomain>()));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        return fixture;
    }

    private static Fixture CreateLicenseeFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<Licensee>(u => u
            .With(s => s.Reseller, new Reseller())
            .With(s => s.Brands,
                Array.Empty<Brand>())
            .With(s => s.GameProviders,
                Array.Empty<LicenseeGameProvider>())
            .With(s => s.Languages,
                Array.Empty<LicenseeLanguage>())
            .With(s => s.Currencies,
                Array.Empty<LicenseeCurrency>())
            .With(s => s.BetLimits,
                Array.Empty<LicenseeBetLimit>())
            .With(s => s.Departments,
                Array.Empty<Department>()));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        return fixture;
    }

    private static Fixture CreateMenuSettingFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<MenuSetting>(u => u
            .With(s => s.SubMenus,
                Array.Empty<MenuSetting>()));

        fixture.Customize<MenuSetting>(u => u
            .With(s => s.SubMenus, fixture.CreateMany<MenuSetting>(5).ToArray()));

        return fixture;
    }

    private static Fixture CreateResellerFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<Reseller>(u => u
            .With(s => s.GameProviders,
                Array.Empty<ResellerGameProvider>())
            .With(s => s.Languages,
                Array.Empty<ResellerLanguage>())
            .With(s => s.Currencies,
                Array.Empty<ResellerCurrency>())
            .With(s => s.BetLimits,
                Array.Empty<ResellerBetLimit>())
            .With(s => s.Licensees,
                Array.Empty<Licensee>())
            .With(s => s.Departments,
                Array.Empty<Department>()));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        return fixture;
    }

    private static Fixture CreateRoleFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<Role>(u => u
            .With(s => s.Users,
                Array.Empty<User>())
            .With(s => s.MenuSetting));

        return fixture;
    }

    private static Fixture CreateRoleRequestFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<CreateRoleRequest>(u => u
            .With(s => s.Menus,
                Array.Empty<MenuSetting>()));

        return fixture;
    }

    private static Fixture UpdateRoleRequestFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<UpdateRoleRequest>(u => u
            .With(s => s.Menus,
                Array.Empty<MenuSetting>()));

        return fixture;
    }
}
