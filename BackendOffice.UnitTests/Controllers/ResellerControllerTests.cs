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
using TGS.Apis.BackendOffice.Controllers.Messages.Reseller;
using TGS.Apis.BackendOffice.Controllers.Reseller;
using TGS.Apis.BackendOffice.DataService.Services;
using TGS.Core.DataServices.Tgs.Models;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class ResellerControllerTests : BaseTests
{
    private readonly Mock<IResellerService> _resellerServiceMock = new();

    private ResellerController _controller = default!;

    [Test]
    public async Task Can_Create_Reseller()
    {
        var request = new Fixture().Create<CreateResellerRequest>();

        _ = _resellerServiceMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Reseller, bool>>>()))
            .Returns(Task.FromResult(false));

        var response = await _controller.CreateResellerAsync(request);
        _ = response.Valid.Should().BeTrue();

        _resellerServiceMock.Verify(s => s.ExistsAsync(It.IsAny<Expression<Func<Reseller, bool>>>()));
        _resellerServiceMock.Verify(p => p.CreateResellerAsync(It.Is<CreateResellerRequest>(r =>
                r.Name == request.Name),
            It.Is<string>(r => r == Username)));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Resellers(bool showAll)
    {
        var request = new GetResellersRequest
        {
            Keyword = "keyword",
            Page = 1,
            Rows = 10,
            SortBy = "updatedOn",
            OrderBy = SortOrder.Descending,
            ShowAll = showAll
        };

        var list = CreateResellerFixture()
            .CreateMany<Reseller>(105)
            .AsQueryable()
            .BuildMock();

        var (skip, take) = GetPage(request);
        var orderedRoles = list.OrderByDescending(p => p.UpdatedOn);

        var expectedRoles = showAll ? orderedRoles.ToArray() : orderedRoles.Skip(skip).Take(take).ToArray();

        _ = _resellerServiceMock.Setup(x => x.GetResellers(It.IsAny<GetResellersRequest>(), false))
            .Returns(list);

        var response = await _controller.GetResellersAsync(request);
        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            _ = response.Items[i].Id.Should().Be(expectedRoles[i].Id);
            _ = response.Items[i].Name.Should().Be(expectedRoles[i].Name);
        }

        _resellerServiceMock.Verify(p => p.GetResellers(It.IsAny<GetResellersRequest>(), false));
    }

    [Test]
    public async Task Can_Update_Reseller()
    {
        var reseller = CreateResellerFixture().Create<Reseller>();
        var request = new Fixture().Create<UpdateResellerRequest>();

        _ = _resellerServiceMock.Setup(s => s.GetResellerAsync(It.IsAny<Expression<Func<Reseller, bool>>>(),
                It.IsAny<Func<IQueryable<Reseller>, IQueryable<Reseller>>>()))
            .Returns(Task.FromResult(reseller));

        _ = _resellerServiceMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<Reseller, bool>>>()))
            .Returns(Task.FromResult(false));

        var response = await _controller.UpdateResellerAsync(reseller.Id, request);

        _ = response.Valid.Should().BeTrue();
        _resellerServiceMock.Verify(s => s.GetResellerAsync(It.IsAny<Expression<Func<Reseller, bool>>>(),
            It.IsAny<Func<IQueryable<Reseller>, IQueryable<Reseller>>>()));
        _resellerServiceMock.Verify(p => p.UpdateResellerAsync(It.Is<Reseller>(r =>
                r.Name == reseller.Name),
            It.IsAny<UpdateResellerRequest>(),
            It.Is<string>(r => r == Username)));
    }

    [Test]
    public async Task Cannot_Create_Reseller_With_Code_Exists()
    {
        var list = CreateResellerFixture()
            .CreateMany<Reseller>(2)
            .AsQueryable()
            .BuildMock();

        var request = new Fixture().Create<CreateResellerRequest>();
        request.Code = list.FirstOrDefault()?.Code;

        _ = _resellerServiceMock.Setup(s =>
                s.ExistsAsync(It.IsAny<Expression<Func<Reseller, bool>>>()))
            .Returns((Expression<Func<Reseller, bool>> predicate) => list.AnyAsync(predicate));

        var response = await _controller.CreateResellerAsync(request);
        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Reseller.CodeExists);
    }

    [Test]
    public async Task Cannot_Create_Reseller_With_Name_Exists()
    {
        var list = CreateResellerFixture()
            .CreateMany<Reseller>(2)
            .AsQueryable()
            .BuildMock();

        var request = new Fixture().Create<CreateResellerRequest>();
        request.Name = list.FirstOrDefault()?.Name;

        _ = _resellerServiceMock.Setup(s =>
                s.ExistsAsync(It.IsAny<Expression<Func<Reseller, bool>>>()))
            .Returns((Expression<Func<Reseller, bool>> predicate) => list.AnyAsync(predicate));

        var response = await _controller.CreateResellerAsync(request);
        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Reseller.NameExists);
    }

    [Test]
    public async Task Cannot_Update_Reseller_When_Not_Found()
    {
        var reseller = CreateResellerFixture().Create<Reseller>();
        var request = new Fixture().Create<UpdateResellerRequest>();

        _ = _resellerServiceMock.Setup(s => s.GetResellerAsync(
                It.IsAny<Expression<Func<Reseller, bool>>>(),
                It.IsAny<Func<IQueryable<Reseller>,
                    IQueryable<Reseller>>>()))
            .Returns(Task.FromResult<Reseller>(default!));

        _ = _resellerServiceMock.Setup(s =>
                s.ExistsAsync(It.IsAny<Expression<Func<Reseller, bool>>>()))
            .Returns(Task.FromResult(false));

        var response = await _controller.UpdateResellerAsync(reseller.Id, request);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Reseller.NotFound);
        _resellerServiceMock.Verify(s => s.GetResellerAsync(
            It.IsAny<Expression<Func<Reseller, bool>>>(),
            It.IsAny<Func<IQueryable<Reseller>,
                IQueryable<Reseller>>>()));
        _resellerServiceMock.Verify(p => p.UpdateResellerAsync(It.Is<Reseller>(r =>
                r.Name == reseller.Name),
            It.IsAny<UpdateResellerRequest>(),
            It.Is<string>(r => r == Username)), Times.Never);
    }

    [Test]
    public async Task Cannot_Update_Reseller_With_Name_Exists()
    {
        var reseller = CreateResellerFixture().Create<Reseller>();
        var request = new Fixture().Create<UpdateResellerRequest>();
        request.Name = reseller.Name;
        _ = _resellerServiceMock.Setup(s => s.GetResellerAsync(
                It.IsAny<Expression<Func<Reseller, bool>>>(),
                It.IsAny<Func<IQueryable<Reseller>,
                    IQueryable<Reseller>>>()))
            .Returns(Task.FromResult(reseller));

        _ = _resellerServiceMock.Setup(s =>
                s.ExistsAsync(It.IsAny<Expression<Func<Reseller, bool>>>()))
            .Returns(Task.FromResult(true));

        var response = await _controller.UpdateResellerAsync(reseller.Id, request);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Reseller.NameExists);
        _resellerServiceMock.Verify(s => s.GetResellerAsync(
            It.IsAny<Expression<Func<Reseller, bool>>>(),
            It.IsAny<Func<IQueryable<Reseller>,
                IQueryable<Reseller>>>()));
        _resellerServiceMock.Verify(p => p.UpdateResellerAsync(It.Is<Reseller>(r =>
                r.Name == reseller.Name),
            It.IsAny<UpdateResellerRequest>(),
            It.Is<string>(r => r == Username)), Times.Never);
    }

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _controller = new ResellerController(
            _resellerServiceMock.Object);
        _controller.ControllerContext.HttpContext = HttpContextMock.Object;
    }

    private static Fixture CreateResellerFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<Reseller>(u => u
            .With(s => s.GameProviders, Array.Empty<ResellerGameProvider>())
            .With(s => s.Languages, Array.Empty<ResellerLanguage>())
            .With(s => s.Currencies, Array.Empty<ResellerCurrency>())
            .With(s => s.BetLimits, Array.Empty<ResellerBetLimit>())
            .With(s => s.Licensees, Array.Empty<Licensee>())
            .With(s => s.Departments, Array.Empty<Department>()));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}
