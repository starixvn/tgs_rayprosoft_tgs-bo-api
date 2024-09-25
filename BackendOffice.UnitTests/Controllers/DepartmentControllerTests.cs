using System.Linq.Expressions;
using AutoFixture;
using FluentAssertions;
using MockQueryable.Moq;
using TGS.Apis.BackendOffice.Controllers;
using TGS.Apis.BackendOffice.Controllers.Messages.Department;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class DepartmentControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Can_Create_Department()
    {
        var commandsMock = Substitute.For<ITgsCommands<Department>>();

        var queriesMock = Substitute.For<ITgsQueries<Department>>();

        var controller = new DepartmentController(queriesMock, commandsMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var request = new Fixture().Create<DepartmentRequest>();

        var departments = CreateDepartmentFixture().CreateMany<Department>(10).AsQueryable()
            .BuildMock();

        _ = queriesMock.GetQueries().Returns(departments);

        var response = await controller.CreateAsync(request);

        _ = response.Valid.Should().BeTrue();

        _ = commandsMock.Received(1).CreateAsync(Arg.Is<Department>(d => d.Name == request.Name));
    }

    [Test]
    public async Task Can_Delete_Department()
    {
        var commandsMock = Substitute.For<ITgsCommands<Department>>();

        var queriesMock = Substitute.For<ITgsQueries<Department>>();

        var controller = new DepartmentController(queriesMock, commandsMock);

        var department = CreateDepartmentFixture().Create<Department>();

        var departmentId = Guid.NewGuid();

        _ = queriesMock.GetSingleAsync(Arg.Any<Expression<Func<Department, bool>>>(),
                Arg.Any<Func<IQueryable<Department>, IQueryable<Department>>>())
            .Returns(Task.FromResult(department));

        var response = await controller.DeleteAsync(departmentId);

        _ = response.Valid.Should().BeTrue();

        _ = queriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Department, bool>>>(),
            Arg.Any<Func<IQueryable<Department>, IQueryable<Department>>>());

        _ = commandsMock.Received(1).DeleteAsync(Arg.Is<Department>(r =>
            r.Name == department.Name));
    }

    [Test]
    public async Task Can_Delete_Department_With_Users_Used()
    {
        var commandsMock = Substitute.For<ITgsCommands<Department>>();

        var queriesMock = Substitute.For<ITgsQueries<Department>>();

        var controller = new DepartmentController(queriesMock, commandsMock);

        var department = CreateDepartmentFixture().Create<Department>();

        var departmentId = Guid.NewGuid();

        department.Users =
        [
            new User()
        ];

        _ = queriesMock.GetSingleAsync(Arg.Any<Expression<Func<Department, bool>>>(),
                Arg.Any<Func<IQueryable<Department>, IQueryable<Department>>>())
            .Returns(Task.FromResult(department));

        var response = await controller.DeleteAsync(departmentId);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Department.CannotBeDeleted);

        _ = queriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Department, bool>>>(),
            Arg.Any<Func<IQueryable<Department>, IQueryable<Department>>>());

        _ = commandsMock.Received(0).DeleteAsync(Arg.Is<Department>(r =>
            r.Name == department.Name));
    }    

    [Test]
    public async Task Can_Update_Department()
    {
        var commandsMock = Substitute.For<ITgsCommands<Department>>();

        var queriesMock = Substitute.For<ITgsQueries<Department>>();

        var controller = new DepartmentController(queriesMock, commandsMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var department = CreateDepartmentFixture().Create<Department>();

        var request = new Fixture().Create<DepartmentRequest>();

        var list = CreateDepartmentFixture()
            .CreateMany<Department>(105)
            .AsQueryable()
            .BuildMock();

        _ = queriesMock.GetQueries().Returns(list);

        _ = queriesMock.GetSingleAsync(Arg.Any<Expression<Func<Department, bool>>>(),
                Arg.Any<Func<IQueryable<Department>, IQueryable<Department>>>())
            .Returns(Task.FromResult(department));

        _ = queriesMock.ExistsAsync(Arg.Any<Expression<Func<Department, bool>>>())
            .Returns(Task.FromResult(false));

        var response = await controller.UpdateAsync(department.Id, request);

        _ = response.Valid.Should().BeTrue();

        _ = queriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Department, bool>>>(),
            Arg.Any<Func<IQueryable<Department>, IQueryable<Department>>>());

        _ = commandsMock.Received(1).UpdateAsync(Arg.Is<Department>(r =>
            r.Name == department.Name));
    }

    [Test]
    public async Task Can_Update_Department_Not_Found()
    {
        var commandsMock = Substitute.For<ITgsCommands<Department>>();

        var queriesMock = Substitute.For<ITgsQueries<Department>>();

        var controller = new DepartmentController(queriesMock, commandsMock);

        var department = CreateDepartmentFixture().Create<Department>();

        var request = new Fixture().Create<DepartmentRequest>();

        _ = queriesMock.GetSingleAsync(Arg.Any<Expression<Func<Department, bool>>>(),
                Arg.Any<Func<IQueryable<Department>, IQueryable<Department>>>())
            .Returns(Task.FromResult<Department>(default!));

        _ = queriesMock.ExistsAsync(Arg.Any<Expression<Func<Department, bool>>>())
            .Returns(Task.FromResult(false));

        var response = await controller.UpdateAsync(department.Id, request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Department.NotFound);

        _ = queriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Department, bool>>>(),
            Arg.Any<Func<IQueryable<Department>, IQueryable<Department>>>());

        _ = commandsMock.Received(0).UpdateAsync(Arg.Is<Department>(r =>
            r.Name == department.Name));
    }

    [Test]
    public async Task Cannot_Create_Department_With_Name_Exists()
    {
        var commandsMock = Substitute.For<ITgsCommands<Department>>();

        var queriesMock = Substitute.For<ITgsQueries<Department>>();

        var controller = new DepartmentController(queriesMock, commandsMock);

        var request = new Fixture().Create<DepartmentRequest>();

        var departments = CreateDepartmentFixture().CreateMany<Department>(10).AsQueryable()
            .BuildMock();

        request.Name = "Name";
        request.ResellerId = null;
        request.LicenseeId = null;
        request.BrandId = null;
        departments.First().Name = "Name";
        departments.First().ResellerId = null;
        departments.First().LicenseeId = null;
        departments.First().BrandId = null;

        _ = queriesMock.GetQueries().Returns(departments);

        var response = await controller.CreateAsync(request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Department.NameExists);
    }

    [Test]
    public async Task Cannot_Delete_Department_Not_Found()
    {
        var commandsMock = Substitute.For<ITgsCommands<Department>>();

        var queriesMock = Substitute.For<ITgsQueries<Department>>();

        var controller = new DepartmentController(queriesMock, commandsMock);

        var department = CreateDepartmentFixture().Create<Department>();

        var departmentId = Guid.NewGuid();

        _ = queriesMock.GetSingleAsync(Arg.Any<Expression<Func<Department, bool>>>(),
                Arg.Any<Func<IQueryable<Department>, IQueryable<Department>>>())
            .Returns(Task.FromResult<Department>(default!));

        var response = await controller.DeleteAsync(departmentId);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Department.NotFound);

        _ = queriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Department, bool>>>(),
            Arg.Any<Func<IQueryable<Department>, IQueryable<Department>>>());

        _ = commandsMock.Received(0).DeleteAsync(Arg.Is<Department>(r =>
            r.Name == department.Name));
    }

    [Test]
    public async Task Cannot_Update_Department_With_Name_Exists()
    {
        var commandsMock = Substitute.For<ITgsCommands<Department>>();

        var queriesMock = Substitute.For<ITgsQueries<Department>>();

        var controller = new DepartmentController(queriesMock, commandsMock);

        var department = CreateDepartmentFixture().Create<Department>();

        var request = new Fixture().Create<DepartmentRequest>();

        var departments = CreateDepartmentFixture().CreateMany<Department>(10).AsQueryable()
            .BuildMock();

        request.Name = "Name";
        department.ResellerId = null;
        department.LicenseeId = null;
        department.BrandId = null;

        var firstDepartment = departments.First();
        firstDepartment.Name = "Name";
        firstDepartment.ResellerId = null;
        firstDepartment.LicenseeId = null;
        firstDepartment.BrandId = null;

        _ = queriesMock.GetSingleAsync(Arg.Any<Expression<Func<Department, bool>>>(),
                Arg.Any<Func<IQueryable<Department>, IQueryable<Department>>>())
            .Returns(Task.FromResult(department));

        _ = queriesMock.GetQueries().Returns(departments);

        var response = await controller.UpdateAsync(department.Id, request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Department.NameExists);

        _ = queriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Department, bool>>>(),
            Arg.Any<Func<IQueryable<Department>, IQueryable<Department>>>());

        _ = commandsMock.Received(0).UpdateAsync(Arg.Is<Department>(r =>
            r.Name == department.Name));
    }

    private static Fixture CreateDepartmentFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<Department>(u => u
            .With(s => s.Users, [])
            .With(s => s.Reseller, new Reseller())
            .With(s => s.Licensee, new Licensee())
            .With(s => s.Brand, new Brand()));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}