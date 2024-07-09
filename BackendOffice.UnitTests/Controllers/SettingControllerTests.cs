using System.Data.SqlClient;
using System.Linq.Expressions;
using AutoFixture;
using FluentAssertions;
using MockQueryable.Moq;
using NSubstitute;
using TGS.Apis.BackendOffice.Controllers;
using TGS.Apis.BackendOffice.Controllers.Messages.Settings;
using TGS.Apis.BackendOffice.Resources;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.Infrastructures.Redis;
using TGS.Core.Models.Messages;
using Setting = TGS.Core.DataServices.Tgs.Models.Setting;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class SettingControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Can_Create_Setting()
    {
        var commandsMock = Substitute.For<ITgsCommands<Setting>>();

        var queriesMock = Substitute.For<ITgsQueries<Setting>>();

        var redisCacheManagerMock = Substitute.For<IRedisCacheManager>();

        var controller = new SettingController(queriesMock, commandsMock, redisCacheManagerMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var request = new Fixture().Create<CreateSettingRequest>();

        _ = queriesMock.GetSingleAsync(Arg.Any<Expression<Func<Setting, bool>>>(),
                Arg.Any<Func<IQueryable<Setting>, IQueryable<Setting>>>())
            .Returns(Task.FromResult<Setting>(default!));

        var response = await controller.CreateAsync(request);
        _ = response.Valid.Should().BeTrue();

        _ = queriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Setting, bool>>>(),
            Arg.Any<Func<IQueryable<Setting>, IQueryable<Setting>>>());

        _ = commandsMock.Received(1).CreateAsync(Arg.Is<Setting>(d =>
            d.Key == request.Key &&
            d.Value == request.Value &&
            d.Description == request.Description));
    }

    [Test]
    public async Task Can_Delete_Setting()
    {
        var commandsMock = Substitute.For<ITgsCommands<Setting>>();

        var queriesMock = Substitute.For<ITgsQueries<Setting>>();

        var redisCacheManagerMock = Substitute.For<IRedisCacheManager>();

        var controller = new SettingController(queriesMock, commandsMock, redisCacheManagerMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var key = Generator.RandomString(6);

        var setting = new Fixture().Create<Setting>();

        setting.Key = key;

        _ = queriesMock.GetSingleAsync(Arg.Any<Expression<Func<Setting, bool>>>(),
                Arg.Any<Func<IQueryable<Setting>, IQueryable<Setting>>>())
            .Returns(Task.FromResult(setting));

        var response = await controller.DeleteAsync(key);
        _ = response.Valid.Should().BeTrue();

        _ = queriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Setting, bool>>>(),
            Arg.Any<Func<IQueryable<Setting>, IQueryable<Setting>>>());

        _ = commandsMock.Received(1).DeleteAsync(Arg.Is<Setting>(d =>
            d.Key == key));
    }

    [Test]
    public async Task Can_Get_Setting()
    {
        var commandsMock = Substitute.For<ITgsCommands<Setting>>();

        var queriesMock = Substitute.For<ITgsQueries<Setting>>();

        var redisCacheManagerMock = Substitute.For<IRedisCacheManager>();

        var controller = new SettingController(queriesMock, commandsMock, redisCacheManagerMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var key = Generator.RandomString(6);

        var setting = new Fixture().Create<Setting>();

        setting.Key = key;

        _ = queriesMock.GetSingleAsync(Arg.Any<Expression<Func<Setting, bool>>>(),
                Arg.Any<Func<IQueryable<Setting>, IQueryable<Setting>>>())
            .Returns(Task.FromResult(setting));

        var response = await controller.GetSingleAsync(key);

        _ = response.Valid.Should().BeTrue();

        _ = response.Setting.Key.Should().Be(setting.Key);
        _ = response.Setting.Value.Should().Be(setting.Value);
        _ = response.Setting.Description.Should().Be(setting.Description);
        _ = response.Setting.CreatedByUsername.Should().Be(setting.CreatedByUsername);
        _ = response.Setting.UpdatedByUsername.Should().Be(setting.UpdatedByUsername);
        _ = response.Setting.CreatedOn.Should().Be(setting.CreatedOn);
        _ = response.Setting.UpdatedOn.Should().Be(setting.UpdatedOn);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Settings(bool showAll)
    {
        var commandsMock = Substitute.For<ITgsCommands<Setting>>();

        var queriesMock = Substitute.For<ITgsQueries<Setting>>();

        var redisCacheManagerMock = Substitute.For<IRedisCacheManager>();

        var controller = new SettingController(queriesMock, commandsMock, redisCacheManagerMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var request = new GetSettingsRequest
        {
            Page = 1,
            Rows = 10,
            SortBy = "UpdatedOn",
            OrderBy = SortOrder.Descending,
            ShowAll = showAll
        };

        var list = new Fixture()
            .CreateMany<Setting>(105)
            .AsQueryable()
            .BuildMock();

        var (skip, take) = GetPage(request);
        var orderedRoles = list.OrderByDescending(p => p.UpdatedOn);

        var expectedRoles = showAll ? [.. orderedRoles] : orderedRoles.Skip(skip).Take(take).ToArray();

        _ = queriesMock.GetQueries().Returns(list);

        var response = await controller.GetListAsync(request);

        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            _ = response.Items[i].Key.Should().Be(expectedRoles[i].Key);
            _ = response.Items[i].Value.Should().Be(expectedRoles[i].Value);
            _ = response.Items[i].Description.Should().Be(expectedRoles[i].Description);
            _ = response.Items[i].CreatedByUsername.Should().Be(expectedRoles[i].CreatedByUsername);
            _ = response.Items[i].UpdatedByUsername.Should().Be(expectedRoles[i].UpdatedByUsername);
            _ = response.Items[i].CreatedOn.Should().Be(expectedRoles[i].CreatedOn);
            _ = response.Items[i].UpdatedOn.Should().Be(expectedRoles[i].UpdatedOn);
        }

        _ = queriesMock.Received(1).GetQueries();
    }

    [Test]
    public async Task Can_Update_Setting()
    {
        var commandsMock = Substitute.For<ITgsCommands<Setting>>();

        var queriesMock = Substitute.For<ITgsQueries<Setting>>();

        var redisCacheManagerMock = Substitute.For<IRedisCacheManager>();

        var controller = new SettingController(queriesMock, commandsMock, redisCacheManagerMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var key = Generator.RandomString(6);

        var request = new Fixture().Create<UpdateSettingRequest>();

        var setting = new Fixture().Create<Setting>();

        _ = queriesMock.GetSingleAsync(Arg.Any<Expression<Func<Setting, bool>>>(),
                Arg.Any<Func<IQueryable<Setting>, IQueryable<Setting>>>())
            .Returns(Task.FromResult(setting));

        var response = await controller.UpdateAsync(key, request);

        _ = response.Valid.Should().BeTrue();

        _ = queriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Setting, bool>>>(),
            Arg.Any<Func<IQueryable<Setting>, IQueryable<Setting>>>());

        _ = commandsMock.Received(1).UpdateAsync(Arg.Is<Setting>(d =>
            d.Value == request.Value &&
            d.Description == request.Description));
    }

    [Test]
    public async Task Cannot_Create_Setting_With_Exists()
    {
        var commandsMock = Substitute.For<ITgsCommands<Setting>>();

        var queriesMock = Substitute.For<ITgsQueries<Setting>>();

        var redisCacheManagerMock = Substitute.For<IRedisCacheManager>();

        var controller = new SettingController(queriesMock, commandsMock, redisCacheManagerMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var request = new Fixture().Create<CreateSettingRequest>();

        _ = queriesMock.GetSingleAsync(Arg.Any<Expression<Func<Setting, bool>>>(),
                Arg.Any<Func<IQueryable<Setting>, IQueryable<Setting>>>())
            .Returns(Task.FromResult(new Setting()));

        var response = await controller.CreateAsync(request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Setting.KeyExists);

        _ = queriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Setting, bool>>>(),
            Arg.Any<Func<IQueryable<Setting>, IQueryable<Setting>>>());

        _ = commandsMock.Received(0).CreateAsync(Arg.Is<Setting>(d =>
            d.Key == request.Key &&
            d.Value == request.Value &&
            d.Description == request.Description));
    }

    [Test]
    public async Task Cannot_Get_Setting_Not_Found()
    {
        var commandsMock = Substitute.For<ITgsCommands<Setting>>();

        var queriesMock = Substitute.For<ITgsQueries<Setting>>();

        var redisCacheManagerMock = Substitute.For<IRedisCacheManager>();

        var controller = new SettingController(queriesMock, commandsMock, redisCacheManagerMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var key = Generator.RandomString(6);

        var setting = new Fixture().Create<Setting>();

        setting.Key = key;

        _ = queriesMock.GetSingleAsync(Arg.Any<Expression<Func<Setting, bool>>>(),
                Arg.Any<Func<IQueryable<Setting>, IQueryable<Setting>>>())
            .Returns(Task.FromResult<Setting>(default!));

        var response = await controller.GetSingleAsync(key);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Common.NotFound);
    }

    [Test]
    public async Task Cannot_Update_Setting()
    {
        var commandsMock = Substitute.For<ITgsCommands<Setting>>();

        var queriesMock = Substitute.For<ITgsQueries<Setting>>();

        var redisCacheManagerMock = Substitute.For<IRedisCacheManager>();

        var controller = new SettingController(queriesMock, commandsMock, redisCacheManagerMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var key = Generator.RandomString(6);

        var request = new Fixture().Create<UpdateSettingRequest>();

        var setting = new Fixture().Create<Setting>();

        _ = queriesMock.GetSingleAsync(Arg.Any<Expression<Func<Setting, bool>>>(),
                Arg.Any<Func<IQueryable<Setting>, IQueryable<Setting>>>())
            .Returns(Task.FromResult(setting));

        var response = await controller.UpdateAsync(key, request);
        _ = response.Valid.Should().BeTrue();

        _ = queriesMock.GetSingleAsync(Arg.Any<Expression<Func<Setting, bool>>>(),
            Arg.Any<Func<IQueryable<Setting>, IQueryable<Setting>>>());

        _ = commandsMock.Received(1).UpdateAsync(Arg.Is<Setting>(d =>
            d.Value == request.Value &&
            d.Description == request.Description));
    }
}