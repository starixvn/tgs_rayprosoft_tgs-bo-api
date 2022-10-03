using System;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
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
    public override void Setup()
    {
        base.Setup();
        _controller = new(
            _queriesMock.Object,
            _commandsMock.Object);
        _controller.ControllerContext.HttpContext = HttpContextMock.Object;
    }

    private DepartmentController _controller=default!;
    private readonly Mock<ITgsQueries<Department>> _queriesMock = new();
    private readonly Mock<ITgsCommands<Department>> _commandsMock = new();

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Departments(bool showAll)
    {
        var request = new GetDepartmentsRequest
        {
            Page = 1,
            Rows = 10,
            SortBy = "Name",
            OrderBy = SortOrder.Descending,
            ShowAll = showAll
        };

        var list = CreateDepartmentFixture()
            .CreateMany<Department>(105)
            .AsQueryable()
            .BuildMock();

        var (skip, take) = GetPage(request);
        var orderedRoles = list.OrderByDescending(p => p.Name);

        var expectedRoles = showAll ? orderedRoles.ToArray() : orderedRoles.Skip(skip).Take(take).ToArray();

        _queriesMock.Setup(x => x.GetQueries())
            .Returns(list);

        var response = await _controller.GetListAsync(request);
        response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            response.Items[i].Id.Should().Be(expectedRoles[i].Id);
            response.Items[i].Name.Should().Be(expectedRoles[i].Name);
        }

        _queriesMock.Verify(p => p.GetQueries());
    }

    [Test]
    public async Task Cannot_Create_Department_With_Name_Exists()
    {
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

        _queriesMock.Setup(s => s.GetQueries())
            .Returns(departments);

        var response = await _controller.CreateAsync(request);
        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Apis.BackendOffice.Resources.Department.NameExists);
    }

    [Test]
    public async Task Can_Create_Department()
    {
        var request = new Fixture().Create<DepartmentRequest>();
        var departments = CreateDepartmentFixture().CreateMany<Department>(10).AsQueryable()
            .BuildMock();

        _queriesMock.Setup(s => s.GetQueries())
            .Returns(departments);

        var response = await _controller.CreateAsync(request);
        response.Valid.Should().BeTrue();

        _commandsMock.Verify(p => p.CreateAsync(It.Is<Department>(d => d.Name == request.Name)));
    }

    [Test]
    public async Task Can_Update_Department()
    {
        var department = CreateDepartmentFixture().Create<Department>();
        var request = new Fixture().Create<DepartmentRequest>();

        _queriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Department, bool>>>(),
                It.IsAny<Func<IQueryable<Department>, IQueryable<Department>>>()))
            .Returns(Task.FromResult(department));

        _queriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Department, bool>>>()))
            .Returns(Task.FromResult(false));

        var response = await _controller.UpdateAsync(department.Id, request);

        response.Valid.Should().BeTrue();
        _queriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Department, bool>>>(),
            It.IsAny<Func<IQueryable<Department>, IQueryable<Department>>>()));
        _commandsMock.Verify(p => p.UpdateAsync(It.Is<Department>(r =>
            r.Name == department.Name)));
    }

    [Test]
    public async Task Can_Update_Department_Not_Found()
    {
        var department = CreateDepartmentFixture().Create<Department>();
        var request = new Fixture().Create<DepartmentRequest>();

        _queriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Department, bool>>>(),
                It.IsAny<Func<IQueryable<Department>, IQueryable<Department>>>()))
            .Returns(Task.FromResult<Department>(default!));

        _queriesMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Department, bool>>>()))
            .Returns(Task.FromResult(false));

        var response = await _controller.UpdateAsync(department.Id, request);

        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Apis.BackendOffice.Resources.Department.NotFound);
        _queriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Department, bool>>>(),
            It.IsAny<Func<IQueryable<Department>, IQueryable<Department>>>()));
        _commandsMock.Verify(p => p.UpdateAsync(It.Is<Department>(r =>
            r.Name == department.Name)), Times.Never);
    }

    [Test]
    public async Task Cannot_Update_Department_With_Name_Exists()
    {
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

        _queriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Department, bool>>>(),
                It.IsAny<Func<IQueryable<Department>, IQueryable<Department>>>()))
            .Returns(Task.FromResult(department));

        _queriesMock.Setup(s => s.GetQueries())
            .Returns(departments);

        var response = await _controller.UpdateAsync(department.Id, request);

        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Apis.BackendOffice.Resources.Department.NameExists);
        _queriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Department, bool>>>(),
            It.IsAny<Func<IQueryable<Department>, IQueryable<Department>>>()));
        _commandsMock.Verify(p => p.UpdateAsync(It.Is<Department>(r =>
            r.Name == department.Name)), Times.Never);
    }

    [Test]
    public async Task Can_Delete_Department()
    {
        var department = CreateDepartmentFixture().Create<Department>();
        var departmentId = Guid.NewGuid();

        _queriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Department, bool>>>(),
                It.IsAny<Func<IQueryable<Department>, IQueryable<Department>>>()))
            .Returns(Task.FromResult(department));

        var response = await _controller.DeleteAsync(departmentId);

        response.Valid.Should().BeTrue();
        _queriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Department, bool>>>(),
            It.IsAny<Func<IQueryable<Department>, IQueryable<Department>>>()));
        _commandsMock.Verify(p => p.DeleteAsync(It.Is<Department>(r =>
            r.Name == department.Name)));
    }


    [Test]
    public async Task Cannot_Delete_Department_Not_Found()
    {
        var department = CreateDepartmentFixture().Create<Department>();
        var departmentId = Guid.NewGuid();

        _queriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Department, bool>>>(),
                It.IsAny<Func<IQueryable<Department>, IQueryable<Department>>>()))
            .Returns(Task.FromResult<Department>(default!));

        var response = await _controller.DeleteAsync(departmentId);

        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Apis.BackendOffice.Resources.Department.NotFound);
        _queriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Department, bool>>>(),
            It.IsAny<Func<IQueryable<Department>, IQueryable<Department>>>()));
        _commandsMock.Verify(p => p.DeleteAsync(It.Is<Department>(r =>
            r.Name == department.Name)), Times.Never);
    }

    [Test]
    public async Task Can_Delete_Department_With_Users_Used()
    {
        var department = CreateDepartmentFixture().Create<Department>();
        var departmentId = Guid.NewGuid();
        department.Users = new[]
        {
            new User()
        };

        _queriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<Department, bool>>>(),
                It.IsAny<Func<IQueryable<Department>, IQueryable<Department>>>()))
            .Returns(Task.FromResult(department));

        var response = await _controller.DeleteAsync(departmentId);

        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Apis.BackendOffice.Resources.Department.CannotBeDeleted);
        _queriesMock.Verify(s => s.GetSingleAsync(It.IsAny<Expression<Func<Department, bool>>>(),
            It.IsAny<Func<IQueryable<Department>, IQueryable<Department>>>()));
        _commandsMock.Verify(p => p.DeleteAsync(It.Is<Department>(r =>
            r.Name == department.Name)), Times.Never);
    }

    private Fixture CreateDepartmentFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<Department>(u => u
            .With(s => s.Users, new User[]
            {
            })
            .With(s => s.Reseller, new Reseller())
            .With(s => s.Licensee, new Licensee())
            .With(s => s.Brand, new Brand()));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}