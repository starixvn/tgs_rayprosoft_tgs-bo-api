using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using NSubstitute;
using TGS.Apis.BackendOffice.Controllers;
using TGS.Apis.BackendOffice.Controllers.Messages.Roles;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Json;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Core.Models.Services;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class RoleControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Can_Copy_Role()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var role = CreateRoleFixture().Create<Role>();
        var request = new Fixture().Create<CopyRoleRequest>();
        role.IsRoot = false;
        var roles = CreateRoleFixture().CreateMany<Role>(10).AsQueryable().BuildMock();

        _ = roleQueriesMock.GetQueries().Returns(roles);

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(role));

        var response = await controller.CopyAsync(role.Id, request);

        _ = response.Valid.Should().BeTrue();

        _ = roleQueriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
            Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>());

        _ = roleCommandsMock.Received(1).CreateAsync(Arg.Is<Role>(r =>
            r.Name == request.Name &&
            r.DisplayName == role.DisplayName &&
            r.Description == role.Description));
    }

    [Test]
    public async Task Can_Create_Role()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var request = CreateRoleRequestFixture().Create<CreateRoleRequest>();

        var roles = CreateRoleFixture().CreateMany<Role>(10).AsQueryable().BuildMock();

        _ = roleQueriesMock.GetQueries().Returns(roles);

        _ = roleQueriesMock.ExistsAsync(Arg.Any<Expression<Func<Role, bool>>>())
            .Returns(Task.FromResult(false));

        var response = await controller.CreateAsync(request);

        _ = response.Valid.Should().BeTrue();

        _ = roleCommandsMock.Received(1).CreateAsync(Arg.Is<Role>(p =>
            p.Name == request.Name &&
            p.DisplayName == request.DisplayName &&
            p.Description == request.Description));
    }

    [Test]
    public async Task Can_Delete_Role()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var role = CreateRoleFixture().Create<Role>();

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(role));

        role.Users = new List<User>();
        role.IsRoot = false;

        var response = await controller.DeleteAsync(role.Id);

        _ = response.Valid.Should().BeTrue();

        _ = roleQueriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
            Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>());

        _ = roleCommandsMock.Received(1).DeleteAsync(Arg.Is<Role>(r =>
            r.Name == role.Name &&
            r.DisplayName == role.DisplayName &&
            r.Description == role.Description));
    }

    [Test]
    public async Task Can_Get_Menus()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var role = CreateRoleFixture().Create<Role>();

        role.Users = new List<User> { new() };

        var request = new GetMenuRequest { RoleType = RoleType.Admin };

        _ = settingQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Setting, bool>>>(),
                Arg.Any<Func<IQueryable<Setting>, IQueryable<Setting>>>())
            .Returns(Task.FromResult(new Setting { Value = "{}" }));

        var menuSettings = CreateMenuSettingFixture().CreateMany<MenuSetting>(10).ToArray();

        _ = jsonSerializationProviderMock
            .DeserializeFromString<MenuSetting[]>(Arg.Any<string>(), Arg.Any<JsonSerializerOptions>())
            .Returns(menuSettings);

        var response = await controller.GetMenuAsync(request);

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
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var role = CreateRoleFixture().Create<Role>();

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(role));

        _ = settingQueriesMock.GetSingleAsync(p => p.Key == "Menus")
            .Returns(Task.FromResult(new Fixture().Create<Setting>()));

        var menuSettings = CreateMenuSettingFixture().CreateMany<MenuSetting>(10).ToArray();

        _ = jsonSerializationProviderMock
            .DeserializeFromString<MenuSetting[]>(Arg.Any<string>(), Arg.Any<JsonSerializerOptions>())
            .Returns(menuSettings);

        var response = await controller.GetSingleAsync(role.Id);
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
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

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

        _ = roleQueriesMock.GetQueries().Returns(roles);

        _ = resellerQueriesMock.GetQueries().Returns(resellers);

        _ = licenseeQueriesMock.GetQueries().Returns(licensees);

        _ = brandQueriesMock.GetQueries().Returns(brands);

        var response = await controller.GetListAsync(request);

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

        _ = roleQueriesMock.Received(1).GetQueries();
    }

    [Test]
    public async Task Can_Not_Delete_Role()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var role = CreateRoleFixture().Create<Role>();

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(role));

        var response = await controller.DeleteAsync(role.Id);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.CannotDeleteRootRole);

        _ = roleQueriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
            Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>());
    }

    [Test]
    public async Task Can_Not_Delete_Role_When_bind_With_Users()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var role = CreateRoleFixture().Create<Role>();

        role.Users = new List<User> { new() };

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(role));

        var response = await controller.DeleteAsync(role.Id);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.CannotDeleteRole);

        _ = roleQueriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
            Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>());
    }

    [Test]
    public async Task Can_Update_Admin_Role()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

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

        _ = jsonSerializationProviderMock
            .DeserializeFromString<MenuSetting[]>(Arg.Any<string>(), Arg.Any<JsonSerializerOptions>())
            .Returns(menuSettings);

        _ = jsonSerializationProviderMock.SerializeToString(Arg.Any<MenuSetting[]>(), Arg.Any<JsonSerializerOptions>())
            .Returns(role.MenuSetting);

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(role));

        _ = roleQueriesMock.ExistsAsync(Arg.Any<Expression<Func<Role, bool>>>())
            .Returns(Task.FromResult(false));

        _ = roleQueriesMock.GetQueries().Returns(roles);

        var response = await controller.UpdateAsync(role.Id, request);

        _ = response.Valid.Should().BeTrue();

        _ = roleQueriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
            Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>());

        _ = roleCommandsMock.Received(1).UpdateAsync(Arg.Is<Role>(r => r.MenuSetting == role.MenuSetting));
    }

    [Test]
    public async Task Can_Update_Brand_Not_Root_Role()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

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

        _ = jsonSerializationProviderMock
            .DeserializeFromString<MenuSetting[]>(Arg.Any<string>(), Arg.Any<JsonSerializerOptions>())
            .Returns(menuSettings);

        _ = jsonSerializationProviderMock.SerializeToString(Arg.Any<MenuSetting[]>(), Arg.Any<JsonSerializerOptions>())
            .Returns(role.MenuSetting);

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(role));

        _ = roleQueriesMock.ExistsAsync(Arg.Any<Expression<Func<Role, bool>>>())
            .Returns(Task.FromResult(false));

        _ = roleQueriesMock.GetQueries().Returns(roles);

        var response = await controller.UpdateAsync(role.Id, request);

        _ = response.Valid.Should().BeTrue();

        _ = roleQueriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
            Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>());

        _ = roleCommandsMock.Received(1).UpdateAsync(Arg.Is<Role>(r =>
            r.MenuSetting == role.MenuSetting));

        _ = roleCommandsMock.Received(0).BatchUpdateAsync(Arg.Is<Role[]>(r =>
            r.Length == 2));
    }

    [Test]
    public async Task Can_Update_Brand_Root_Role()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var roles = CreateRoleFixture()
            .CreateMany<Role>(10)
            .AsQueryable()
            .BuildMock();
        var index = 0;
        var role = new Role();

        await roles.ForEachAsync(x =>
        {
            switch (index)
            {
                case 0:
                    x.IsRoot = true;
                    x.RoleType = RoleType.Brand;
                    role = x;
                    break;
                case 2:
                    x.IsRoot = true;
                    x.RoleType = RoleType.Licensee;
                    x.LicenseeId = role.LicenseeId;
                    break;
                case 3:
                    x.IsRoot = false;
                    x.RoleType = RoleType.Licensee;
                    x.LicenseeId = role.LicenseeId;
                    break;
                case 4:
                    x.IsRoot = false;
                    x.RoleType = RoleType.Brand;
                    x.BrandId = role.BrandId;
                    break;
            }

            index++;
        });

        var request = UpdateRoleRequestFixture().Create<UpdateRoleRequest>();

        var menuSettings = CreateMenuSettingFixture().CreateMany<MenuSetting>(10).ToArray();

        _ = jsonSerializationProviderMock
            .DeserializeFromString<MenuSetting[]>(Arg.Any<string>(), Arg.Any<JsonSerializerOptions>())
            .Returns(menuSettings);

        _ = jsonSerializationProviderMock.SerializeToString(Arg.Any<MenuSetting[]>(), Arg.Any<JsonSerializerOptions>())
            .Returns(role.MenuSetting);

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(role));

        _ = roleQueriesMock.ExistsAsync(Arg.Any<Expression<Func<Role, bool>>>())
            .Returns(Task.FromResult(false));

        _ = roleQueriesMock.GetQueries().Returns(roles);

        var response = await controller.UpdateAsync(role.Id, request);

        _ = response.Valid.Should().BeTrue();

        _ = roleQueriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
            Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>());

        _ = roleCommandsMock.Received(1).BatchUpdateAsync(Arg.Is<Role[]>(r =>
            r.Length == 2));
    }

    [Test]
    public async Task Can_Update_Licensee_Not_Root_Role()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

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

            switch (index)
            {
                case 2:
                    x.IsRoot = true;
                    x.RoleType = RoleType.Licensee;
                    licenseeRootRole = x;
                    break;
                case 3:
                    x.IsRoot = false;
                    x.RoleType = RoleType.Licensee;
                    x.LicenseeId = licenseeRootRole.Id;
                    role = x;
                    break;
                case 4:
                    x.IsRoot = true;
                    x.RoleType = RoleType.Licensee;
                    break;
            }

            index++;
        });

        var request = UpdateRoleRequestFixture().Create<UpdateRoleRequest>();

        var menuSettings = CreateMenuSettingFixture().CreateMany<MenuSetting>(10).ToArray();

        _ = jsonSerializationProviderMock
            .DeserializeFromString<MenuSetting[]>(Arg.Any<string>(), Arg.Any<JsonSerializerOptions>())
            .Returns(menuSettings);

        _ = jsonSerializationProviderMock.SerializeToString(Arg.Any<MenuSetting[]>(), Arg.Any<JsonSerializerOptions>())
            .Returns(role.MenuSetting);

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(role));

        _ = roleQueriesMock.ExistsAsync(Arg.Any<Expression<Func<Role, bool>>>())
            .Returns(Task.FromResult(false));

        _ = roleQueriesMock.GetQueries().Returns(roles);

        var response = await controller.UpdateAsync(role.Id, request);

        _ = response.Valid.Should().BeTrue();

        _ = roleQueriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
            Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>());

        _ = roleCommandsMock.Received(1).UpdateAsync(Arg.Is<Role>(r =>
            r.MenuSetting == role.MenuSetting));

        _ = roleCommandsMock.Received(0).BatchUpdateAsync(Arg.Is<Role[]>(r =>
            r.Length == 1));
    }

    [Test]
    public async Task Can_Update_Licensee_Root_Role()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

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

            switch (index)
            {
                case 2:
                    x.IsRoot = true;
                    x.RoleType = RoleType.Licensee;
                    role = x;
                    break;
                case 3:
                    x.IsRoot = false;
                    x.RoleType = RoleType.Licensee;
                    x.LicenseeId = role.LicenseeId;
                    break;
                case 4:
                    x.IsRoot = true;
                    x.RoleType = RoleType.Brand;
                    x.LicenseeId = role.LicenseeId;
                    break;
            }

            index++;
        });

        var request = UpdateRoleRequestFixture().Create<UpdateRoleRequest>();

        var menuSettings = CreateMenuSettingFixture().CreateMany<MenuSetting>(10).ToArray();

        _ = jsonSerializationProviderMock
            .DeserializeFromString<MenuSetting[]>(Arg.Any<string>(), Arg.Any<JsonSerializerOptions>())
            .Returns(menuSettings);

        _ = jsonSerializationProviderMock.SerializeToString(Arg.Any<MenuSetting[]>(), Arg.Any<JsonSerializerOptions>())
            .Returns(role.MenuSetting);

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(role));

        _ = roleQueriesMock.ExistsAsync(Arg.Any<Expression<Func<Role, bool>>>())
            .Returns(Task.FromResult(false));

        _ = roleQueriesMock.GetQueries().Returns(roles);

        var response = await controller.UpdateAsync(role.Id, request);

        _ = response.Valid.Should().BeTrue();

        _ = roleQueriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
            Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>());

        _ = roleCommandsMock.Received(1).BatchUpdateAsync(Arg.Is<Role[]>(r =>
            r.Length == 3));
    }

    [Test]
    public async Task Can_Update_Reseller_Not_Root_Role()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var roles = CreateRoleFixture()
            .CreateMany<Role>(10)
            .AsQueryable()
            .BuildMock();

        var index = 0;

        var role = new Role();

        var resellerRootRole = new Role();

        await roles.ForEachAsync(x =>
        {
            switch (index)
            {
                case 2:
                    x.IsRoot = true;
                    x.RoleType = RoleType.Reseller;
                    resellerRootRole = x;
                    break;
                case 3:
                    x.IsRoot = false;
                    x.RoleType = RoleType.Reseller;
                    x.ResellerId = resellerRootRole.ResellerId;
                    role = x;
                    break;
                case 4:
                    x.IsRoot = true;
                    x.RoleType = RoleType.Licensee;
                    x.ResellerId = resellerRootRole.ResellerId;
                    break;
                case 5:
                    x.IsRoot = true;
                    x.RoleType = RoleType.Brand;
                    x.ResellerId = resellerRootRole.ResellerId;
                    break;
            }

            index++;
        });

        var request = UpdateRoleRequestFixture().Create<UpdateRoleRequest>();

        var menuSettings = CreateMenuSettingFixture().CreateMany<MenuSetting>(10).ToArray();

        _ = jsonSerializationProviderMock
            .DeserializeFromString<MenuSetting[]>(Arg.Any<string>(), Arg.Any<JsonSerializerOptions>())
            .Returns(menuSettings);

        _ = jsonSerializationProviderMock.SerializeToString(Arg.Any<MenuSetting[]>(), Arg.Any<JsonSerializerOptions>())
            .Returns(role.MenuSetting);

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(role));

        _ = roleQueriesMock.ExistsAsync(Arg.Any<Expression<Func<Role, bool>>>())
            .Returns(Task.FromResult(false));

        _ = roleQueriesMock.GetQueries().Returns(roles);

        var response = await controller.UpdateAsync(role.Id, request);

        _ = response.Valid.Should().BeTrue();

        _ = roleQueriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
            Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>());

        _ = roleCommandsMock.Received(1).UpdateAsync(Arg.Is<Role>(r =>
            r.MenuSetting == role.MenuSetting));

        _ = roleCommandsMock.Received(0).BatchUpdateAsync(Arg.Is<Role[]>(r =>
            r.Length == 1));
    }

    [Test]
    public async Task Can_Update_Reseller_Root_Role()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

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

            switch (index)
            {
                case 2:
                    x.IsRoot = true;
                    x.RoleType = RoleType.Reseller;
                    role = x;
                    break;
                case 3:
                    x.IsRoot = true;
                    x.RoleType = RoleType.Licensee;
                    x.ResellerId = role.ResellerId;
                    break;
            }

            index++;
        });

        var request = UpdateRoleRequestFixture().Create<UpdateRoleRequest>();

        var menuSettings = CreateMenuSettingFixture().CreateMany<MenuSetting>(10).ToArray();

        _ = jsonSerializationProviderMock
            .DeserializeFromString<MenuSetting[]>(Arg.Any<string>(), Arg.Any<JsonSerializerOptions>())
            .Returns(menuSettings);

        _ = jsonSerializationProviderMock.SerializeToString(Arg.Any<MenuSetting[]>(), Arg.Any<JsonSerializerOptions>())
            .Returns(role.MenuSetting);

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(role));

        _ = roleQueriesMock.ExistsAsync(Arg.Any<Expression<Func<Role, bool>>>())
            .Returns(Task.FromResult(false));

        _ = roleQueriesMock.GetQueries().Returns(roles);

        var response = await controller.UpdateAsync(role.Id, request);

        _ = response.Valid.Should().BeTrue();

        _ = roleQueriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
            Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>());

        _ = roleCommandsMock.Received(1).BatchUpdateAsync(Arg.Is<Role[]>(r =>
            r.Length == 2));
    }

    [Test]
    public async Task Cannot_Copy_Role_Not_Found()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var role = CreateRoleFixture().Create<Role>();

        var request = new Fixture().Create<CopyRoleRequest>();

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult<Role>(default!));

        _ = roleQueriesMock.ExistsAsync(Arg.Any<Expression<Func<Role, bool>>>())
            .Returns(Task.FromResult(true));

        var response = await controller.CopyAsync(role.Id, request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NotFound);

        _ = roleCommandsMock.Received(0).UpdateAsync(Arg.Is<Role>(r =>
            r.Name == role.Name &&
            r.DisplayName == role.DisplayName &&
            r.Description == role.Description));
    }

    [Test]
    public async Task Cannot_Copy_Role_With_Name_Exists()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

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

        _ = roleQueriesMock.GetQueries().Returns(roles);

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(role));

        var response = await controller.CopyAsync(role.Id, request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NameExists);

        _ = roleQueriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
            Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>());

        _ = roleCommandsMock.Received(0).UpdateAsync(Arg.Is<Role>(r =>
            r.Name == role.Name &&
            r.DisplayName == role.DisplayName &&
            r.Description == role.Description));
    }

    [Test]
    public async Task Cannot_Create_User_With_Name_Exists()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

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

        _ = roleQueriesMock.GetQueries().Returns(roles);

        var response = await controller.CreateAsync(request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NameExists);
    }

    [Test]
    public async Task Can_Create_User_Of_Different_Layer_With_Name_Exists()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var request = CreateRoleRequestFixture().Create<CreateRoleRequest>();

        var roles = CreateRoleFixture().CreateMany<Role>(10).AsQueryable().BuildMock();

        var resellerId = Guid.NewGuid();
        var licenseeId = Guid.NewGuid();
        var brandId = Guid.NewGuid();

        request.Name = "Name";
        request.ResellerId = resellerId;
        request.LicenseeId = licenseeId;
        request.BrandId = brandId;

        roles.First().Name = "Name";
        roles.First().ResellerId = resellerId;
        roles.First().LicenseeId = null;
        roles.First().BrandId = null;

        _ = roleQueriesMock.GetQueries().Returns(roles);

        var response = await controller.CreateAsync(request);

        _ = response.Valid.Should().BeTrue();
    }

    [Test]
    public async Task Can_Create_Admin_User_Of_Different_Layer_With_Name_Exists()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var request = CreateRoleRequestFixture().Create<CreateRoleRequest>();

        var roles = CreateRoleFixture().CreateMany<Role>(10).AsQueryable().BuildMock();

        var resellerId = Guid.NewGuid();

        request.Name = "Name";
        request.ResellerId = null;
        request.LicenseeId = null;
        request.BrandId = null;
        request.RoleType = RoleType.Admin;

        roles.First().Name = "Name";
        roles.First().ResellerId = resellerId;
        roles.First().LicenseeId = null;
        roles.First().BrandId = null;

        _ = roleQueriesMock.GetQueries().Returns(roles);

        var response = await controller.CreateAsync(request);

        _ = response.Valid.Should().BeTrue();
    }

    [Test]
    public async Task Cannot_Create_Admin_User_Of_Same_Layer_With_Name_Exists()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var request = CreateRoleRequestFixture().Create<CreateRoleRequest>();

        var roles = CreateRoleFixture().CreateMany<Role>(10).AsQueryable().BuildMock();

        request.Name = "Name";
        request.ResellerId = null;
        request.LicenseeId = null;
        request.BrandId = null;
        request.RoleType = RoleType.Admin;

        roles.First().Name = "Name";
        roles.First().ResellerId = null;
        roles.First().LicenseeId = null;
        roles.First().BrandId = null;

        _ = roleQueriesMock.GetQueries().Returns(roles);

        var response = await controller.CreateAsync(request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NameExists);
    }

    [Test]
    public async Task Cannot_Create_User_Of_Same_Layer_With_Name_Exists()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var request = CreateRoleRequestFixture().Create<CreateRoleRequest>();

        var roles = CreateRoleFixture().CreateMany<Role>(10).AsQueryable().BuildMock();

        var resellerId = Guid.NewGuid();
        var licenseeId = Guid.NewGuid();

        request.Name = "Name";
        request.ResellerId = resellerId;
        request.LicenseeId = licenseeId;
        request.BrandId = null;

        roles.First().Name = "Name";
        roles.First().ResellerId = resellerId;
        roles.First().LicenseeId = licenseeId;
        roles.First().BrandId = null;

        _ = roleQueriesMock.GetQueries().Returns(roles);

        var response = await controller.CreateAsync(request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NameExists);
    }

    [Test]
    public async Task Cannot_Delete_Role_Not_Found()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var role = CreateRoleFixture().Create<Role>();

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult<Role>(default!));

        var response = await controller.DeleteAsync(role.Id);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NotFound);

        _ = roleQueriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
            Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>());

        _ = roleCommandsMock.Received(0).DeleteAsync(Arg.Is<Role>(r =>
            r.Name == role.Name &&
            r.DisplayName == role.DisplayName &&
            r.Description == role.Description));
    }

    [Test]
    public async Task Cannot_Delete_Role_With_User_Used()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var role = CreateRoleFixture().Create<Role>();

        role.Users = new List<User> { new() };

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(role));

        var response = await controller.DeleteAsync(role.Id);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.CannotDeleteRole);

        _ = roleQueriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
            Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>());

        _ = roleCommandsMock.Received(0).DeleteAsync(Arg.Is<Role>(r =>
            r.Name == role.Name &&
            r.DisplayName == role.DisplayName &&
            r.Description == role.Description));
    }

    [Test]
    public async Task Cannot_Get_Menus()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var role = CreateRoleFixture().Create<Role>();

        var request = new GetMenuRequest { RoleType = RoleType.Admin };

        role.Users = new List<User> { new() };

        _ = settingQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Setting, bool>>>(),
                Arg.Any<Func<IQueryable<Setting>, IQueryable<Setting>>>())
            .Returns(Task.FromResult<Setting>(default!));

        var response = await controller.GetMenuAsync(request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.MenuNotFound);
    }

    [Test]
    public async Task Cannot_Get_Menus_When_Is_Licensee_Or_Brand_Role()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var role = CreateRoleFixture().Create<Role>();

        var request = new GetMenuRequest { RoleType = RoleType.Licensee };

        role.Users = new List<User> { new() };

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult<Role>(default!));

        var response = await controller.GetMenuAsync(request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.MenuNotFound);
    }

    [Test]
    public async Task Cannot_Get_Role()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var roleId = Guid.NewGuid();

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult<Role>(default!));

        var response = await controller.GetSingleAsync(roleId);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NotFound);
    }

    [Test]
    public async Task Cannot_Update_Role_Not_Found()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var role = CreateRoleFixture().Create<Role>();

        var request = UpdateRoleRequestFixture().Create<UpdateRoleRequest>();

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult<Role>(default!));

        _ = roleQueriesMock.ExistsAsync(Arg.Any<Expression<Func<Role, bool>>>())
            .Returns(Task.FromResult(true));

        var response = await controller.UpdateAsync(role.Id, request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NotFound);

        _ = roleCommandsMock.Received(0).UpdateAsync(Arg.Is<Role>(r =>
            r.Name == role.Name &&
            r.DisplayName == role.DisplayName &&
            r.Description == role.Description));
    }

    [Test]
    public async Task Can_Update_Role_With_Lower_Level_Name_Exists()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var role = CreateRoleFixture().Create<Role>();

        var request = UpdateRoleRequestFixture().Create<UpdateRoleRequest>();

        var roles = CreateRoleFixture().CreateMany<Role>(10).AsQueryable().BuildMock();

        var resellerId = Guid.NewGuid();
        var licenseeId = Guid.NewGuid();
        var brandId = Guid.NewGuid();

        request.Name = "Name";
        role.ResellerId = resellerId;
        role.LicenseeId = licenseeId;
        role.BrandId = null;

        var firstRole = roles.First();
        firstRole.Name = "Name";
        firstRole.ResellerId = resellerId;
        firstRole.LicenseeId = licenseeId;
        firstRole.BrandId = brandId;

        _ = roleQueriesMock.GetQueries().Returns(roles);

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(role));

        _ = roleQueriesMock.ExistsAsync(Arg.Any<Expression<Func<Role, bool>>>())
            .Returns(Task.FromResult(true));

        var response = await controller.UpdateAsync(role.Id, request);

        _ = response.Valid.Should().BeTrue();

        _ = response.Message.Should().NotBe(Apis.BackendOffice.Resources.Role.NameExists);

        _ = roleCommandsMock.Received(1).UpdateAsync(Arg.Is<Role>(r =>
            r.Name == role.Name &&
            r.DisplayName == role.DisplayName &&
            r.Description == role.Description));
    }

    [Test]
    public async Task Cannot_Update_Admin_Role_With_Same_Layer_Name_Exists()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

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

        _ = roleQueriesMock.GetQueries().Returns(roles);

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(role));

        _ = roleQueriesMock.ExistsAsync(Arg.Any<Expression<Func<Role, bool>>>())
            .Returns(Task.FromResult(true));

        var response = await controller.UpdateAsync(role.Id, request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NameExists);

        _ = roleCommandsMock.Received(0).UpdateAsync(Arg.Is<Role>(r =>
            r.Name == role.Name &&
            r.DisplayName == role.DisplayName &&
            r.Description == role.Description));
    }

    [Test]
    public async Task Can_Update_Role_With_Upper_Level_Name_Exists()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var role = CreateRoleFixture().Create<Role>();

        var request = UpdateRoleRequestFixture().Create<UpdateRoleRequest>();

        var roles = CreateRoleFixture().CreateMany<Role>(10).AsQueryable().BuildMock();

        var resellerId = Guid.NewGuid();
        var licenseeId = Guid.NewGuid();

        request.Name = "Name";
        role.ResellerId = resellerId;
        role.LicenseeId = licenseeId;
        role.BrandId = null;

        var firstRole = roles.First();
        firstRole.Name = "Name";
        firstRole.ResellerId = resellerId;
        firstRole.LicenseeId = null;
        firstRole.BrandId = null;

        _ = roleQueriesMock.GetQueries().Returns(roles);

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(role));

        _ = roleQueriesMock.ExistsAsync(Arg.Any<Expression<Func<Role, bool>>>())
            .Returns(Task.FromResult(true));

        var response = await controller.UpdateAsync(role.Id, request);

        _ = response.Valid.Should().BeTrue();

        _ = response.Message.Should().NotBe(Apis.BackendOffice.Resources.Role.NameExists);

        _ = roleCommandsMock.Received(1).UpdateAsync(Arg.Is<Role>(r =>
            r.Name == role.Name &&
            r.DisplayName == role.DisplayName &&
            r.Description == role.Description));
    }

    [Test]
    public async Task Cannot_Update_Role_With_Name_Exists()
    {
        var brandQueriesMock = Substitute.For<ITgsQueries<Brand>>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var licenseeQueriesMock = Substitute.For<ITgsQueries<Licensee>>();

        var resellerQueriesMock = Substitute.For<ITgsQueries<Reseller>>();

        var roleCommandsMock = Substitute.For<ITgsCommands<Role>>();

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();

        var settingQueriesMock = Substitute.For<ITgsQueries<Setting>>();

        var controller =
            new RoleController(roleQueriesMock, settingQueriesMock, roleCommandsMock, resellerQueriesMock,
                licenseeQueriesMock, brandQueriesMock, jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

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

        _ = roleQueriesMock.GetQueries().Returns(roles);

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(role));

        _ = roleQueriesMock.ExistsAsync(Arg.Any<Expression<Func<Role, bool>>>())
            .Returns(Task.FromResult(true));

        var response = await controller.UpdateAsync(role.Id, request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NameExists);

        _ = roleCommandsMock.Received(0).UpdateAsync(Arg.Is<Role>(r =>
            r.Name == role.Name &&
            r.DisplayName == role.DisplayName &&
            r.Description == role.Description));
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
            .With(s => s.SubMenus, []));

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
            .With(s => s.Menus, []));

        return fixture;
    }

    private static Fixture UpdateRoleRequestFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<UpdateRoleRequest>(u => u
            .With(s => s.Menus, []));

        return fixture;
    }
}