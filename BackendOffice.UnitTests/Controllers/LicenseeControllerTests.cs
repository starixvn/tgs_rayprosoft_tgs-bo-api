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
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _controller = new LicenseeController(
            _licenseeServiceMock.Object);
        _controller.ControllerContext.HttpContext = HttpContextMock.Object;
    }

    private LicenseeController _controller = default!;
    private readonly Mock<ILicenseeService> _licenseeServiceMock = new();

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

        _licenseeServiceMock.Setup(x => x.GetLicensees(It.IsAny<GetLicenseesRequest>(), false))
            .Returns(list);

        var response = await _controller.GetLicenseesAsync(request);
        response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            response.Items[i].Id.Should().Be(expectedRoles[i].Id);
            response.Items[i].Name.Should().Be(expectedRoles[i].Name);
        }

        _licenseeServiceMock.Verify(p => p.GetLicensees(It.IsAny<GetLicenseesRequest>(), false));
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

        _licenseeServiceMock.Setup(s =>
                s.ExistsAsync(It.IsAny<Expression<Func<Licensee, bool>>>()))
            .Returns((Expression<Func<Licensee, bool>> predicate) => list.AnyAsync(predicate));

        var response = await _controller.CreateLicenseeAsync(request);
        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Apis.BackendOffice.Resources.Licensee.NameExists);
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

        _licenseeServiceMock.Setup(s =>
                s.ExistsAsync(It.IsAny<Expression<Func<Licensee, bool>>>()))
            .Returns((Expression<Func<Licensee, bool>> predicate) => list.AnyAsync(predicate));

        var response = await _controller.CreateLicenseeAsync(request);
        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Apis.BackendOffice.Resources.Licensee.CodeExists);
    }

    [Test]
    public async Task Can_Create_Licensee()
    {
        var request = new Fixture().Create<CreateLicenseeRequest>();

        _licenseeServiceMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Licensee, bool>>>()))
            .Returns(Task.FromResult(false));

        var response = await _controller.CreateLicenseeAsync(request);
        response.Valid.Should().BeTrue();

        _licenseeServiceMock.Verify(s => s.ExistsAsync(It.IsAny<Expression<Func<Licensee, bool>>>()));
        _licenseeServiceMock.Verify(p => p.CreateLicenseeAsync(It.Is<CreateLicenseeRequest>(r =>
                r.Name == request.Name),
            It.Is<string>(r => r == Username)));
    }

    [Test]
    public async Task Can_Update_Licensee()
    {
        var licensee = CreateLicenseeFixture().Create<Licensee>();
        var request = new Fixture().Create<UpdateLicenseeRequest>();

        _licenseeServiceMock.Setup(s => s.GetLicenseeAsync(It.IsAny<Expression<Func<Licensee, bool>>>(),
                It.IsAny<Func<IQueryable<Licensee>, IQueryable<Licensee>>>(), true))
            .Returns(Task.FromResult(licensee));

        _licenseeServiceMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Licensee, bool>>>()))
            .Returns(Task.FromResult(false));

        var response = await _controller.UpdateLicenseeAsync(licensee.Id, request);

        response.Valid.Should().BeTrue();
        _licenseeServiceMock.Verify(s => s.GetLicenseeAsync(It.IsAny<Expression<Func<Licensee, bool>>>(),
            It.IsAny<Func<IQueryable<Licensee>, IQueryable<Licensee>>>(), true));
        _licenseeServiceMock.Verify(p => p.UpdateLicenseeAsync(It.Is<Licensee>(r =>
                r.Name == licensee.Name),
            It.IsAny<UpdateLicenseeRequest>(),
            It.Is<string>(r => r == Username)));
    }

    [Test]
    public async Task Cannot_Update_Licensee_When_Not_Found()
    {
        var licensee = CreateLicenseeFixture().Create<Licensee>();
        var request = new Fixture().Create<UpdateLicenseeRequest>();

        _licenseeServiceMock.Setup(s => s.GetLicenseeAsync(
                It.IsAny<Expression<Func<Licensee, bool>>>(),
                It.IsAny<Func<IQueryable<Licensee>,
                    IQueryable<Licensee>>>(), true))
            .Returns(Task.FromResult<Licensee>(default!));

        _licenseeServiceMock.Setup(s =>
                s.ExistsAsync(It.IsAny<Expression<Func<Licensee, bool>>>()))
            .Returns(Task.FromResult(false));

        var response = await _controller.UpdateLicenseeAsync(licensee.Id, request);

        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Apis.BackendOffice.Resources.Licensee.NotFound);
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
        _licenseeServiceMock.Setup(s => s.GetLicenseeAsync(
                It.IsAny<Expression<Func<Licensee, bool>>>(),
                It.IsAny<Func<IQueryable<Licensee>,
                    IQueryable<Licensee>>>(), true))
            .Returns(Task.FromResult(licensee));

        _licenseeServiceMock.Setup(s =>
                s.ExistsAsync(It.IsAny<Expression<Func<Licensee, bool>>>()))
            .Returns(Task.FromResult(true));

        var response = await _controller.UpdateLicenseeAsync(licensee.Id, request);

        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Apis.BackendOffice.Resources.Licensee.NameExists);
        _licenseeServiceMock.Verify(s => s.GetLicenseeAsync(
            It.IsAny<Expression<Func<Licensee, bool>>>(),
            It.IsAny<Func<IQueryable<Licensee>,
                IQueryable<Licensee>>>(), true));
        _licenseeServiceMock.Verify(p => p.UpdateLicenseeAsync(It.Is<Licensee>(r =>
                r.Name == licensee.Name),
            It.IsAny<UpdateLicenseeRequest>(),
            It.Is<string>(r => r == Username)), Times.Never);
    }

    private Fixture CreateLicenseeFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<Licensee>(u => u
            .With(s => s.Reseller, new Reseller())
            .With(s => s.Brands, new Brand[] { })
            .With(s => s.GameProviders, new LicenseeGameProvider[] { })
            .With(s => s.Languages, new LicenseeLanguage[] { })
            .With(s => s.Currencies, new LicenseeCurrency[] { })
            .With(s => s.BetLimits, new LicenseeBetLimit[] { })
            .With(s => s.Departments, new Department[] { }));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}