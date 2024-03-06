using System.Data.SqlClient;
using System.Linq.Expressions;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using NSubstitute;
using TGS.Apis.BackendOffice.Controllers.Messages.Reseller;
using TGS.Apis.BackendOffice.Controllers.Reseller;
using TGS.Apis.BackendOffice.DataService.Services;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Messages;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class ResellerControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Can_Create_Reseller()
    {
        var resellerServiceMock = Substitute.For<IResellerService>();

        var controller = new ResellerController(resellerServiceMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var request = new Fixture().Create<CreateResellerRequest>();

        _ = resellerServiceMock.ExistsAsync(Arg.Any<Expression<Func<Reseller, bool>>>())
            .Returns(Task.FromResult(false));

        var response = await controller.CreateResellerAsync(request);

        _ = response.Valid.Should().BeTrue();

        _ = resellerServiceMock.Received(2).ExistsAsync(Arg.Any<Expression<Func<Reseller, bool>>>());

        _ = resellerServiceMock.Received(1).CreateResellerAsync(Arg.Is<CreateResellerRequest>(r =>
                r.Name == request.Name),
            Arg.Is<string>(r => r == Username));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Resellers(bool showAll)
    {
        var resellerServiceMock = Substitute.For<IResellerService>();

        var controller = new ResellerController(resellerServiceMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

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

        _ = resellerServiceMock.GetResellers(Arg.Any<GetResellersRequest>())
            .Returns(list);

        var response = await controller.GetResellersAsync(request);
        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            _ = response.Items[i].Id.Should().Be(expectedRoles[i].Id);
            _ = response.Items[i].Name.Should().Be(expectedRoles[i].Name);
        }

        _ = resellerServiceMock.Received(1).GetResellers(Arg.Any<GetResellersRequest>());
    }

    [Test]
    public async Task Can_Update_Reseller()
    {
        var resellerServiceMock = Substitute.For<IResellerService>();

        var controller = new ResellerController(resellerServiceMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var reseller = CreateResellerFixture().Create<Reseller>();

        var request = new Fixture().Create<UpdateResellerRequest>();

        _ = resellerServiceMock.GetResellerAsync(Arg.Any<Expression<Func<Reseller, bool>>>(),
                Arg.Any<Func<IQueryable<Reseller>, IQueryable<Reseller>>>())
            .Returns(Task.FromResult(reseller));

        _ = resellerServiceMock.ExistsAsync(Arg.Any<Expression<Func<Reseller, bool>>>())
            .Returns(Task.FromResult(false));

        var response = await controller.UpdateResellerAsync(reseller.Id, request);

        _ = response.Valid.Should().BeTrue();

        _ = resellerServiceMock.Received(1).GetResellerAsync(Arg.Any<Expression<Func<Reseller, bool>>>(),
            Arg.Any<Func<IQueryable<Reseller>, IQueryable<Reseller>>>());

        _ = resellerServiceMock.Received(1).UpdateResellerAsync(Arg.Is<Reseller>(r =>
                r.Name == reseller.Name),
            Arg.Any<UpdateResellerRequest>(),
            Arg.Is<string>(r => r == Username));
    }

    [Test]
    public async Task Cannot_Create_Reseller_With_Code_Exists()
    {
        var resellerServiceMock = Substitute.For<IResellerService>();

        var controller = new ResellerController(resellerServiceMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var list = CreateResellerFixture()
            .CreateMany<Reseller>(2)
            .AsQueryable()
            .BuildMock();

        var request = new Fixture().Create<CreateResellerRequest>();

        request.Code = list.FirstOrDefault()?.Code;

        _ = resellerServiceMock.ExistsAsync(Arg.Any<Expression<Func<Reseller, bool>>>())
            .Returns(x => list.AnyAsync(x.Arg<Expression<Func<Reseller, bool>>>()));

        var response = await controller.CreateResellerAsync(request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Reseller.CodeExists);
    }

    [Test]
    public async Task Cannot_Create_Reseller_With_Name_Exists()
    {
        var resellerServiceMock = Substitute.For<IResellerService>();

        var controller = new ResellerController(resellerServiceMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var list = CreateResellerFixture()
            .CreateMany<Reseller>(2)
            .AsQueryable()
            .BuildMock();

        var request = new Fixture().Create<CreateResellerRequest>();
        request.Name = list.FirstOrDefault()?.Name;

        _ = resellerServiceMock.ExistsAsync(Arg.Any<Expression<Func<Reseller, bool>>>())
            .Returns(x => list.AnyAsync(x.Arg<Expression<Func<Reseller, bool>>>()));

        var response = await controller.CreateResellerAsync(request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Reseller.NameExists);
    }

    [Test]
    public async Task Cannot_Update_Reseller_When_Not_Found()
    {
        var resellerServiceMock = Substitute.For<IResellerService>();

        var controller = new ResellerController(resellerServiceMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var reseller = CreateResellerFixture().Create<Reseller>();

        var request = new Fixture().Create<UpdateResellerRequest>();

        _ = resellerServiceMock.GetResellerAsync(
                Arg.Any<Expression<Func<Reseller, bool>>>(),
                Arg.Any<Func<IQueryable<Reseller>,
                    IQueryable<Reseller>>>())
            .Returns(Task.FromResult<Reseller>(default!));

        _ = resellerServiceMock.ExistsAsync(Arg.Any<Expression<Func<Reseller, bool>>>())
            .Returns(Task.FromResult(false));

        var response = await controller.UpdateResellerAsync(reseller.Id, request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Reseller.NotFound);

        _ = resellerServiceMock.Received(1).GetResellerAsync(
            Arg.Any<Expression<Func<Reseller, bool>>>(),
            Arg.Any<Func<IQueryable<Reseller>,
                IQueryable<Reseller>>>());

        _ = resellerServiceMock.Received(0).UpdateResellerAsync(Arg.Is<Reseller>(r =>
                r.Name == reseller.Name),
            Arg.Any<UpdateResellerRequest>(),
            Arg.Is<string>(r => r == Username));
    }

    [Test]
    public async Task Cannot_Update_Reseller_With_Name_Exists()
    {
        var resellerServiceMock = Substitute.For<IResellerService>();

        var controller = new ResellerController(resellerServiceMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var reseller = CreateResellerFixture().Create<Reseller>();

        var request = new Fixture().Create<UpdateResellerRequest>();

        request.Name = reseller.Name;

        _ = resellerServiceMock.GetResellerAsync(
                Arg.Any<Expression<Func<Reseller, bool>>>(),
                Arg.Any<Func<IQueryable<Reseller>,
                    IQueryable<Reseller>>>())
            .Returns(Task.FromResult(reseller));

        _ = resellerServiceMock.ExistsAsync(Arg.Any<Expression<Func<Reseller, bool>>>())
            .Returns(Task.FromResult(true));

        var response = await controller.UpdateResellerAsync(reseller.Id, request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Reseller.NameExists);

        _ = resellerServiceMock.Received(1).GetResellerAsync(
            Arg.Any<Expression<Func<Reseller, bool>>>(),
            Arg.Any<Func<IQueryable<Reseller>,
                IQueryable<Reseller>>>());

        _ = resellerServiceMock.Received(0).UpdateResellerAsync(Arg.Is<Reseller>(r =>
                r.Name == reseller.Name),
            Arg.Any<UpdateResellerRequest>(),
            Arg.Is<string>(r => r == Username));
    }

    private static Fixture CreateResellerFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<Reseller>(u => u
            .With(s => s.GameProviders, [])
            .With(s => s.Languages, [])
            .With(s => s.Currencies, [])
            .With(s => s.BetLimits, [])
            .With(s => s.Licensees, [])
            .With(s => s.Departments, []));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}