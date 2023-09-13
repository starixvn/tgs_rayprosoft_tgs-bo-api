using System.Data.SqlClient;
using System.Linq.Expressions;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using NSubstitute;
using TGS.Apis.BackendOffice.Controllers.Licensee;
using TGS.Apis.BackendOffice.Controllers.Messages.Licensee;
using TGS.Apis.BackendOffice.DataService.Services;
using TGS.Core.DataServices.Tgs.Models;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class LicenseeControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Can_Create_Licensee()
    {
        var licenseeServiceMock = Substitute.For<ILicenseeService>();

        var controller = new LicenseeController(licenseeServiceMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var request = new Fixture().Create<CreateLicenseeRequest>();

        _ = licenseeServiceMock.ExistsAsync(Arg.Any<Expression<Func<Licensee, bool>>>())
            .Returns(Task.FromResult(false));

        var response = await controller.CreateLicenseeAsync(request);

        _ = response.Valid.Should().BeTrue();

        _ = licenseeServiceMock.Received(2).ExistsAsync(Arg.Any<Expression<Func<Licensee, bool>>>());

        _ = licenseeServiceMock.Received(1).CreateLicenseeAsync(Arg.Is<CreateLicenseeRequest>(r =>
                r.Name == request.Name),
            Arg.Is<string>(r => r == Username));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Licensees(bool showAll)
    {
        var licenseeServiceMock = Substitute.For<ILicenseeService>();

        var controller = new LicenseeController(licenseeServiceMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var request = new GetLicenseesRequest
        {
            Keyword = "keyword",
            Page = 1,
            Rows = 10,
            SortBy = "updatedOn",
            OrderBy = SortOrder.Descending,
            ShowAll = showAll
        };

        var list = CreateLicenseeFixture()
            .CreateMany<Licensee>(33)
            .AsQueryable()
            .BuildMock();

        var (skip, take) = GetPage(request);
        var orderedRoles = list.OrderByDescending(p => p.UpdatedOn);

        var expectedRoles = showAll ? orderedRoles.ToArray() : orderedRoles.Skip(skip).Take(take).ToArray();

        _ = licenseeServiceMock.GetLicensees(Arg.Any<GetLicenseesRequest>())
            .Returns(list);

        var response = await controller.GetLicenseesAsync(request);

        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            _ = response.Items[i].Id.Should().Be(expectedRoles[i].Id);
            _ = response.Items[i].Name.Should().Be(expectedRoles[i].Name);
        }

        _ = licenseeServiceMock.Received(1).GetLicensees(Arg.Any<GetLicenseesRequest>());
    }

    [Test]
    public async Task Can_Update_Licensee()
    {
        var licenseeServiceMock = Substitute.For<ILicenseeService>();

        var controller = new LicenseeController(licenseeServiceMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var licensee = CreateLicenseeFixture().Create<Licensee>();

        var request = new Fixture().Create<UpdateLicenseeRequest>();

        _ = licenseeServiceMock.GetLicenseeAsync(Arg.Any<Expression<Func<Licensee, bool>>>(),
                Arg.Any<Func<IQueryable<Licensee>, IQueryable<Licensee>>>(), true)
            .Returns(Task.FromResult(licensee));

        _ = licenseeServiceMock.ExistsAsync(Arg.Any<Expression<Func<Licensee, bool>>>())
            .Returns(Task.FromResult(false));

        var response = await controller.UpdateLicenseeAsync(licensee.Id, request);

        _ = response.Valid.Should().BeTrue();

        _ = licenseeServiceMock.Received(1).GetLicenseeAsync(Arg.Any<Expression<Func<Licensee, bool>>>(),
            Arg.Any<Func<IQueryable<Licensee>, IQueryable<Licensee>>>(), true);

        _ = licenseeServiceMock.Received(1).UpdateLicenseeAsync(Arg.Is<Licensee>(r =>
                r.Name == licensee.Name),
            Arg.Any<UpdateLicenseeRequest>(),
            Arg.Is<string>(r => r == Username));
    }

    [Test]
    public async Task Cannot_Create_Licensee_With_Code_Exists()
    {
        var licenseeServiceMock = Substitute.For<ILicenseeService>();

        var controller = new LicenseeController(licenseeServiceMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var list = CreateLicenseeFixture()
            .CreateMany<Licensee>(2)
            .AsQueryable()
            .BuildMock();

        var request = new Fixture().Create<CreateLicenseeRequest>();
        request.Code = list.FirstOrDefault()?.Code;

        _ = licenseeServiceMock.ExistsAsync(Arg.Any<Expression<Func<Licensee, bool>>>())
            .Returns(x => list.AnyAsync(x.Arg<Expression<Func<Licensee, bool>>>()));

        var response = await controller.CreateLicenseeAsync(request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Licensee.CodeExists);
    }

    [Test]
    public async Task Cannot_Create_Licensee_With_Name_Exists()
    {
        var licenseeServiceMock = Substitute.For<ILicenseeService>();

        var controller = new LicenseeController(licenseeServiceMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var list = CreateLicenseeFixture()
            .CreateMany<Licensee>(2)
            .AsQueryable()
            .BuildMock();

        var request = new Fixture().Create<CreateLicenseeRequest>();
        request.Name = list.FirstOrDefault()?.Name;

        _ = licenseeServiceMock.ExistsAsync(Arg.Any<Expression<Func<Licensee, bool>>>())
            .Returns(x => list.AnyAsync(x.Arg<Expression<Func<Licensee, bool>>>()));

        var response = await controller.CreateLicenseeAsync(request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Licensee.NameExists);
    }

    [Test]
    public async Task Cannot_Update_Licensee_When_Not_Found()
    {
        var licenseeServiceMock = Substitute.For<ILicenseeService>();

        var controller = new LicenseeController(licenseeServiceMock);

        var licensee = CreateLicenseeFixture().Create<Licensee>();

        var request = new Fixture().Create<UpdateLicenseeRequest>();

        _ = licenseeServiceMock.GetLicenseeAsync(
                Arg.Any<Expression<Func<Licensee, bool>>>(),
                Arg.Any<Func<IQueryable<Licensee>,
                    IQueryable<Licensee>>>(), true)
            .Returns(Task.FromResult<Licensee>(default!));

        _ = licenseeServiceMock.ExistsAsync(Arg.Any<Expression<Func<Licensee, bool>>>())
            .Returns(Task.FromResult(false));

        var response = await controller.UpdateLicenseeAsync(licensee.Id, request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Licensee.NotFound);

        _ = licenseeServiceMock.Received(1).GetLicenseeAsync(
            Arg.Any<Expression<Func<Licensee, bool>>>(),
            Arg.Any<Func<IQueryable<Licensee>,
                IQueryable<Licensee>>>(), true);

        _ = licenseeServiceMock.Received(0).UpdateLicenseeAsync(Arg.Is<Licensee>(r =>
                r.Name == licensee.Name),
            Arg.Any<UpdateLicenseeRequest>(),
            Arg.Is<string>(r => r == Username));
    }

    [Test]
    public async Task Cannot_Update_Licensee_With_Name_Exists()
    {
        var licenseeServiceMock = Substitute.For<ILicenseeService>();

        var controller = new LicenseeController(licenseeServiceMock);

        var licensee = CreateLicenseeFixture().Create<Licensee>();

        var request = new Fixture().Create<UpdateLicenseeRequest>();

        request.Name = licensee.Name;

        _ = licenseeServiceMock.GetLicenseeAsync(
                Arg.Any<Expression<Func<Licensee, bool>>>(),
                Arg.Any<Func<IQueryable<Licensee>,
                    IQueryable<Licensee>>>(), true)
            .Returns(Task.FromResult(licensee));

        _ = licenseeServiceMock.ExistsAsync(Arg.Any<Expression<Func<Licensee, bool>>>())
            .Returns(Task.FromResult(true));

        var response = await controller.UpdateLicenseeAsync(licensee.Id, request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Licensee.NameExists);

        _ = licenseeServiceMock.Received(1).GetLicenseeAsync(
            Arg.Any<Expression<Func<Licensee, bool>>>(),
            Arg.Any<Func<IQueryable<Licensee>,
                IQueryable<Licensee>>>(), true);

        _ = licenseeServiceMock.Received(0).UpdateLicenseeAsync(Arg.Is<Licensee>(r =>
                r.Name == licensee.Name),
            Arg.Any<UpdateLicenseeRequest>(),
            Arg.Is<string>(r => r == Username));
    }

    private static Fixture CreateLicenseeFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<Licensee>(u => u
            .With(s => s.Reseller, new Reseller())
            .With(s => s.Brands, Array.Empty<Brand>())
            .With(s => s.GameProviders, Array.Empty<LicenseeGameProvider>())
            .With(s => s.Languages, Array.Empty<LicenseeLanguage>())
            .With(s => s.Currencies, Array.Empty<LicenseeCurrency>())
            .With(s => s.BetLimits, Array.Empty<LicenseeBetLimit>())
            .With(s => s.Departments, Array.Empty<Department>()));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}