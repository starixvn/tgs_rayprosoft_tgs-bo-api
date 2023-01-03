using System;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using TGS.Apis.BackendOffice.Controllers.Licensee;
using TGS.Apis.BackendOffice.Controllers.Messages.Licensee;
using TGS.Apis.BackendOffice.DataService.Services;
using TGS.Core.DataServices.Tgs.Models;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class LicenseeControllerTests : BaseTests
{
    private readonly Mock<ILicenseeService> _licenseeServiceMock = new();

    private LicenseeController _controller = default!;

    [Test]
    public async Task Can_Create_Licensee()
    {
        var request = new Fixture().Create<CreateLicenseeRequest>();

        _ = _licenseeServiceMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Licensee, bool>>>()))
            .Returns(Task.FromResult(false));

        var response = await _controller.CreateLicenseeAsync(request);
        _ = response.Valid.Should().BeTrue();

        _licenseeServiceMock.Verify(s => s.ExistsAsync(It.IsAny<Expression<Func<Licensee, bool>>>()));
        _licenseeServiceMock.Verify(p => p.CreateLicenseeAsync(It.Is<CreateLicenseeRequest>(r =>
                r.Name == request.Name),
            It.Is<string>(r => r == Username)));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Licensees(bool showAll)
    {
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

        _ = _licenseeServiceMock.Setup(x => x.GetLicensees(It.IsAny<GetLicenseesRequest>(), false))
            .Returns(list);

        var response = await _controller.GetLicenseesAsync(request);
        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            _ = response.Items[i].Id.Should().Be(expectedRoles[i].Id);
            _ = response.Items[i].Name.Should().Be(expectedRoles[i].Name);
        }

        _licenseeServiceMock.Verify(p => p.GetLicensees(It.IsAny<GetLicenseesRequest>(), false));
    }

    [Test]
    public async Task Can_Update_Licensee()
    {
        var licensee = CreateLicenseeFixture().Create<Licensee>();
        var request = new Fixture().Create<UpdateLicenseeRequest>();

        _ = _licenseeServiceMock.Setup(s => s.GetLicenseeAsync(It.IsAny<Expression<Func<Licensee, bool>>>(),
                It.IsAny<Func<IQueryable<Licensee>, IQueryable<Licensee>>>(), true))
            .Returns(Task.FromResult(licensee));

        _ = _licenseeServiceMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Licensee, bool>>>()))
            .Returns(Task.FromResult(false));

        var response = await _controller.UpdateLicenseeAsync(licensee.Id, request);

        _ = response.Valid.Should().BeTrue();
        _licenseeServiceMock.Verify(s => s.GetLicenseeAsync(It.IsAny<Expression<Func<Licensee, bool>>>(),
            It.IsAny<Func<IQueryable<Licensee>, IQueryable<Licensee>>>(), true));
        _licenseeServiceMock.Verify(p => p.UpdateLicenseeAsync(It.Is<Licensee>(r =>
                r.Name == licensee.Name),
            It.IsAny<UpdateLicenseeRequest>(),
            It.Is<string>(r => r == Username)));
    }

    [Test]
    public async Task Cannot_Create_Licensee_With_Code_Exists()
    {
        var list = CreateLicenseeFixture()
            .CreateMany<Licensee>(2)
            .AsQueryable()
            .BuildMock();

        var request = new Fixture().Create<CreateLicenseeRequest>();
        request.Code = list.FirstOrDefault()?.Code;

        _ = _licenseeServiceMock.Setup(s =>
                s.ExistsAsync(It.IsAny<Expression<Func<Licensee, bool>>>()))
            .Returns((Expression<Func<Licensee, bool>> predicate) => list.AnyAsync(predicate));

        var response = await _controller.CreateLicenseeAsync(request);
        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Licensee.CodeExists);
    }

    [Test]
    public async Task Cannot_Create_Licensee_With_Name_Exists()
    {
        var list = CreateLicenseeFixture()
            .CreateMany<Licensee>(2)
            .AsQueryable()
            .BuildMock();

        var request = new Fixture().Create<CreateLicenseeRequest>();
        request.Name = list.FirstOrDefault()?.Name;

        _ = _licenseeServiceMock.Setup(s =>
                s.ExistsAsync(It.IsAny<Expression<Func<Licensee, bool>>>()))
            .Returns((Expression<Func<Licensee, bool>> predicate) => list.AnyAsync(predicate));

        var response = await _controller.CreateLicenseeAsync(request);
        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Licensee.NameExists);
    }

    [Test]
    public async Task Cannot_Update_Licensee_When_Not_Found()
    {
        var licensee = CreateLicenseeFixture().Create<Licensee>();
        var request = new Fixture().Create<UpdateLicenseeRequest>();

        _ = _licenseeServiceMock.Setup(s => s.GetLicenseeAsync(
                It.IsAny<Expression<Func<Licensee, bool>>>(),
                It.IsAny<Func<IQueryable<Licensee>,
                    IQueryable<Licensee>>>(), true))
            .Returns(Task.FromResult<Licensee>(default!));

        _ = _licenseeServiceMock.Setup(s =>
                s.ExistsAsync(It.IsAny<Expression<Func<Licensee, bool>>>()))
            .Returns(Task.FromResult(false));

        var response = await _controller.UpdateLicenseeAsync(licensee.Id, request);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Licensee.NotFound);
        _licenseeServiceMock.Verify(s => s.GetLicenseeAsync(
            It.IsAny<Expression<Func<Licensee, bool>>>(),
            It.IsAny<Func<IQueryable<Licensee>,
                IQueryable<Licensee>>>(), true));
        _licenseeServiceMock.Verify(p => p.UpdateLicenseeAsync(It.Is<Licensee>(r =>
                r.Name == licensee.Name),
            It.IsAny<UpdateLicenseeRequest>(),
            It.Is<string>(r => r == Username)), Times.Never);
    }

    [Test]
    public async Task Cannot_Update_Licensee_With_Name_Exists()
    {
        var licensee = CreateLicenseeFixture().Create<Licensee>();
        var request = new Fixture().Create<UpdateLicenseeRequest>();
        request.Name = licensee.Name;
        _ = _licenseeServiceMock.Setup(s => s.GetLicenseeAsync(
                It.IsAny<Expression<Func<Licensee, bool>>>(),
                It.IsAny<Func<IQueryable<Licensee>,
                    IQueryable<Licensee>>>(), true))
            .Returns(Task.FromResult(licensee));

        _ = _licenseeServiceMock.Setup(s =>
                s.ExistsAsync(It.IsAny<Expression<Func<Licensee, bool>>>()))
            .Returns(Task.FromResult(true));

        var response = await _controller.UpdateLicenseeAsync(licensee.Id, request);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Licensee.NameExists);
        _licenseeServiceMock.Verify(s => s.GetLicenseeAsync(
            It.IsAny<Expression<Func<Licensee, bool>>>(),
            It.IsAny<Func<IQueryable<Licensee>,
                IQueryable<Licensee>>>(), true));
        _licenseeServiceMock.Verify(p => p.UpdateLicenseeAsync(It.Is<Licensee>(r =>
                r.Name == licensee.Name),
            It.IsAny<UpdateLicenseeRequest>(),
            It.Is<string>(r => r == Username)), Times.Never);
    }

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _controller = new LicenseeController(
            _licenseeServiceMock.Object);
        _controller.ControllerContext.HttpContext = HttpContextMock.Object;
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
