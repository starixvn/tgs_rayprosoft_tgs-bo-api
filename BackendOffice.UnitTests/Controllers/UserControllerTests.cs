using System.Linq.Expressions;
using AutoFixture;
using FluentAssertions;
using MockQueryable.Moq;
using NSubstitute;
using Tgs.Core.Infrastructures.Security;
using TGS.Apis.BackendOffice.Controllers;
using TGS.Apis.BackendOffice.Controllers.Messages.Users;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class UserControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Can_Create_User()
    {
        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();
        var userQueriesMock = Substitute.For<ITgsQueries<User>>();
        var userCommandsMock = Substitute.For<ITgsCommands<User>>();
        var departmentQueriesMock = Substitute.For<ITgsQueries<Department>>();
        var passwordHandlerMock = Substitute.For<IPasswordHandler>();

        var controller = new UserController(
            passwordHandlerMock,
            userQueriesMock,
            departmentQueriesMock,
            roleQueriesMock,
            userCommandsMock)
        { ControllerContext = { HttpContext = HttpContextMock } };

        var request = new Fixture().Create<CreateUserRequest>();
        var users = CreateUserFixture().CreateMany<User>(10).AsQueryable().BuildMock();

        _ = userQueriesMock.GetQueries().Returns(users);

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(new Role { RoleType = RoleType.Reseller }));

        _ = departmentQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Department, bool>>>(),
                Arg.Any<Func<IQueryable<Department>, IQueryable<Department>>>())
            .Returns(Task.FromResult(new Department()));

        var response = await controller.CreateAsync(request);

        _ = response.Valid.Should().BeTrue();

        _ = userCommandsMock.Received(1).CreateAsync(Arg.Is<User>(p =>
            p.Name == request.Name &&
            p.Username == request.Username &&
            p.RoleId == request.RoleId &&
            p.DepartmentId == request.DepartmentId));
    }

    [Test]
    public async Task Can_Update_User()
    {
        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();
        var userQueriesMock = Substitute.For<ITgsQueries<User>>();
        var userCommandsMock = Substitute.For<ITgsCommands<User>>();
        var departmentQueriesMock = Substitute.For<ITgsQueries<Department>>();
        var passwordHandlerMock = Substitute.For<IPasswordHandler>();

        var controller = new UserController(
            passwordHandlerMock,
            userQueriesMock,
            departmentQueriesMock,
            roleQueriesMock,
            userCommandsMock)
        { ControllerContext = { HttpContext = HttpContextMock } };

        var user = CreateUserFixture().Create<User>();

        var request = new Fixture().Create<UpdateUserRequest>();

        _ = userQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(),
                Arg.Any<Func<IQueryable<User>, IQueryable<User>>>())
            .Returns(Task.FromResult(user));

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(new Role()));

        _ = departmentQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Department, bool>>>(),
                Arg.Any<Func<IQueryable<Department>, IQueryable<Department>>>())
            .Returns(Task.FromResult(new Department()));

        var response = await controller.UpdateAsync(user.Id, request);

        _ = response.Valid.Should().BeTrue();

        _ = userCommandsMock.Received(1).UpdateAsync(Arg.Is<User>(u => u.Name == request.Name));
    }

    [Test]
    public async Task Can_Update_User_Password()
    {
        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();
        var userQueriesMock = Substitute.For<ITgsQueries<User>>();
        var userCommandsMock = Substitute.For<ITgsCommands<User>>();
        var departmentQueriesMock = Substitute.For<ITgsQueries<Department>>();
        var passwordHandlerMock = Substitute.For<IPasswordHandler>();

        var controller = new UserController(
            passwordHandlerMock,
            userQueriesMock,
            departmentQueriesMock,
            roleQueriesMock,
            userCommandsMock)
        { ControllerContext = { HttpContext = HttpContextMock } };

        var user = CreateUserFixture().Create<User>();
        var password = Generator.RandomString(6);

        var request = new UpdatePasswordRequest
        {
            Password = password,
            ConfirmPassword = password
        };

        _ = userQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(),
                Arg.Any<Func<IQueryable<User>, IQueryable<User>>>())
            .Returns(Task.FromResult(user));

        var response = await controller.UpdatePasswordAsync(user.Id, request);

        _ = response.Valid.Should().BeTrue();

        _ = userQueriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<Func<IQueryable<User>, IQueryable<User>>>());

        _ = userCommandsMock.Received(1).UpdateAsync(user);
    }

    [Test]
    public async Task Cannot_Create_User_With_Department_Not_Found()
    {
        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();
        var userQueriesMock = Substitute.For<ITgsQueries<User>>();
        var userCommandsMock = Substitute.For<ITgsCommands<User>>();
        var departmentQueriesMock = Substitute.For<ITgsQueries<Department>>();
        var passwordHandlerMock = Substitute.For<IPasswordHandler>();

        var controller = new UserController(
            passwordHandlerMock,
            userQueriesMock,
            departmentQueriesMock,
            roleQueriesMock,
            userCommandsMock)
        { ControllerContext = { HttpContext = HttpContextMock } };

        var request = new Fixture().Create<CreateUserRequest>();

        var users = CreateUserFixture().CreateMany<User>(10).AsQueryable()
            .BuildMock();

        _ = userQueriesMock.GetQueries().Returns(users);

        _ = userQueriesMock.ExistsAsync(Arg.Any<Expression<Func<User, bool>>>())
            .Returns(Task.FromResult(false));

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(new Role { RoleType = RoleType.Reseller }));

        _ = departmentQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Department, bool>>>(),
                Arg.Any<Func<IQueryable<Department>, IQueryable<Department>>>())
            .Returns(Task.FromResult<Department>(default!));

        var response = await controller.CreateAsync(request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Department.NotFound);
    }

    [Test]
    public async Task Cannot_Create_User_With_Name_Exists()
    {
        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();
        var userQueriesMock = Substitute.For<ITgsQueries<User>>();
        var userCommandsMock = Substitute.For<ITgsCommands<User>>();
        var departmentQueriesMock = Substitute.For<ITgsQueries<Department>>();
        var passwordHandlerMock = Substitute.For<IPasswordHandler>();

        var controller = new UserController(
            passwordHandlerMock,
            userQueriesMock,
            departmentQueriesMock,
            roleQueriesMock,
            userCommandsMock)
        { ControllerContext = { HttpContext = HttpContextMock } };

        var role = new Role
        {
            Id = Guid.NewGuid(),
            RoleType = RoleType.Reseller
        };

        var request = new Fixture().Create<CreateUserRequest>();
        request.RoleId = role.Id;

        var users = CreateUserFixture().CreateMany<User>(10).AsQueryable()
            .BuildMock();
        request.Username = "username";

        var firstUser = users.First();
        firstUser.RoleId = role.Id;
        firstUser.Username = "username";

        _ = userQueriesMock.GetQueries().Returns(users);

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(role));

        var response = await controller.CreateAsync(request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.User.UsernameExistedAlready);
    }

    [Test]
    public async Task Cannot_Create_User_With_Role_Not_Found()
    {
        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();
        var userQueriesMock = Substitute.For<ITgsQueries<User>>();
        var userCommandsMock = Substitute.For<ITgsCommands<User>>();
        var departmentQueriesMock = Substitute.For<ITgsQueries<Department>>();
        var passwordHandlerMock = Substitute.For<IPasswordHandler>();

        var controller = new UserController(
            passwordHandlerMock,
            userQueriesMock,
            departmentQueriesMock,
            roleQueriesMock,
            userCommandsMock)
        { ControllerContext = { HttpContext = HttpContextMock } };

        var request = new Fixture().Create<CreateUserRequest>();

        _ = userQueriesMock.ExistsAsync(Arg.Any<Expression<Func<User, bool>>>())
            .Returns(Task.FromResult(false));

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult<Role>(default!));

        var response = await controller.CreateAsync(request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NotFound);
    }

    [Test]
    public async Task Cannot_Update_User_Not_Found()
    {
        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();
        var userQueriesMock = Substitute.For<ITgsQueries<User>>();
        var userCommandsMock = Substitute.For<ITgsCommands<User>>();
        var departmentQueriesMock = Substitute.For<ITgsQueries<Department>>();
        var passwordHandlerMock = Substitute.For<IPasswordHandler>();

        var controller = new UserController(
            passwordHandlerMock,
            userQueriesMock,
            departmentQueriesMock,
            roleQueriesMock,
            userCommandsMock)
        { ControllerContext = { HttpContext = HttpContextMock } };

        var user = CreateUserFixture().Create<User>();
        var request = new Fixture().Create<UpdateUserRequest>();

        _ = userQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(),
                Arg.Any<Func<IQueryable<User>, IQueryable<User>>>())
            .Returns(Task.FromResult<User>(default!));

        var response = await controller.UpdateAsync(user.Id, request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.User.NotFound);

        _ = userQueriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<Func<IQueryable<User>, IQueryable<User>>>());

        _ = userCommandsMock.Received(0).UpdateAsync(user);
    }

    [Test]
    public async Task Cannot_Update_User_Password_With_User_Not_Found()
    {
        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();
        var userQueriesMock = Substitute.For<ITgsQueries<User>>();
        var userCommandsMock = Substitute.For<ITgsCommands<User>>();
        var departmentQueriesMock = Substitute.For<ITgsQueries<Department>>();
        var passwordHandlerMock = Substitute.For<IPasswordHandler>();

        var controller = new UserController(
            passwordHandlerMock,
            userQueriesMock,
            departmentQueriesMock,
            roleQueriesMock,
            userCommandsMock)
        { ControllerContext = { HttpContext = HttpContextMock } };

        var user = CreateUserFixture().Create<User>();

        var password = Generator.RandomString(6);

        var request = new UpdatePasswordRequest
        {
            Password = password,
            ConfirmPassword = password
        };

        _ = userQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(),
                Arg.Any<Func<IQueryable<User>, IQueryable<User>>>())
            .Returns(Task.FromResult<User>(default!));

        var response = await controller.UpdatePasswordAsync(user.Id, request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.User.NotFound);

        _ = userQueriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<Func<IQueryable<User>, IQueryable<User>>>());

        _ = userCommandsMock.Received(0).UpdateAsync(user);
    }

    [Test]
    public async Task Cannot_Update_User_With_Department_Not_Found()
    {
        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();
        var userQueriesMock = Substitute.For<ITgsQueries<User>>();
        var userCommandsMock = Substitute.For<ITgsCommands<User>>();
        var departmentQueriesMock = Substitute.For<ITgsQueries<Department>>();
        var passwordHandlerMock = Substitute.For<IPasswordHandler>();

        var controller = new UserController(
            passwordHandlerMock,
            userQueriesMock,
            departmentQueriesMock,
            roleQueriesMock,
            userCommandsMock)
        { ControllerContext = { HttpContext = HttpContextMock } };

        var user = CreateUserFixture().Create<User>();

        var request = new Fixture().Create<UpdateUserRequest>();

        _ = userQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(),
                Arg.Any<Func<IQueryable<User>, IQueryable<User>>>())
            .Returns(Task.FromResult(user));

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(new Role()));

        _ = departmentQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Department, bool>>>(),
                Arg.Any<Func<IQueryable<Department>, IQueryable<Department>>>())
            .Returns(Task.FromResult<Department>(default!));

        var response = await controller.UpdateAsync(user.Id, request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Department.NotFound);

        _ = userQueriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<Func<IQueryable<User>, IQueryable<User>>>());

        _ = userCommandsMock.Received(0).UpdateAsync(user);
    }

    [Test]
    public async Task Cannot_Update_User_With_Role_Not_Found()
    {
        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();
        var userQueriesMock = Substitute.For<ITgsQueries<User>>();
        var userCommandsMock = Substitute.For<ITgsCommands<User>>();
        var departmentQueriesMock = Substitute.For<ITgsQueries<Department>>();
        var passwordHandlerMock = Substitute.For<IPasswordHandler>();

        var controller = new UserController(
            passwordHandlerMock,
            userQueriesMock,
            departmentQueriesMock,
            roleQueriesMock,
            userCommandsMock)
        { ControllerContext = { HttpContext = HttpContextMock } };

        var user = CreateUserFixture().Create<User>();
        var request = new Fixture().Create<UpdateUserRequest>();

        _ = userQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(),
                Arg.Any<Func<IQueryable<User>, IQueryable<User>>>())
            .Returns(Task.FromResult(user));

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult<Role>(default!));

        var response = await controller.UpdateAsync(user.Id, request);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Role.NotFound);

        _ = userQueriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<Func<IQueryable<User>, IQueryable<User>>>());

        _ = userCommandsMock.Received(0).UpdateAsync(user);
    }

    [Test]
    public async Task Only_Defined_User_Can_Create_Admin_User()
    {
        Login("hank.wu");

        var roleQueriesMock = Substitute.For<ITgsQueries<Role>>();
        var userQueriesMock = Substitute.For<ITgsQueries<User>>();
        var userCommandsMock = Substitute.For<ITgsCommands<User>>();
        var departmentQueriesMock = Substitute.For<ITgsQueries<Department>>();
        var passwordHandlerMock = Substitute.For<IPasswordHandler>();

        var controller = new UserController(
            passwordHandlerMock,
            userQueriesMock,
            departmentQueriesMock,
            roleQueriesMock,
            userCommandsMock)
        { ControllerContext = { HttpContext = HttpContextMock } };

        var users = CreateUserFixture().CreateMany<User>(10).AsQueryable().BuildMock();

        var request = new Fixture().Create<CreateUserRequest>();

        _ = userQueriesMock.GetQueries().Returns(users);

        _ = departmentQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Department, bool>>>(),
                Arg.Any<Func<IQueryable<Department>, IQueryable<Department>>>())
            .Returns(Task.FromResult(new Department()));

        _ = roleQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<Role, bool>>>(),
                Arg.Any<Func<IQueryable<Role>, IQueryable<Role>>>())
            .Returns(Task.FromResult(new Role { RoleType = RoleType.Admin }));

        var response = await controller.CreateAsync(request);

        _ = response.Valid.Should().BeTrue();

        _ = userCommandsMock.Received(1).CreateAsync(Arg.Any<User>());
    }

    private static Fixture CreateUserFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<User>(u => u
            .With(s => s.Role,
                new Role { Name = Generator.RandomString(5) })
            .With(p => p.Department,
                new Department { Name = Generator.RandomString(5) }));

        return fixture;
    }
}
