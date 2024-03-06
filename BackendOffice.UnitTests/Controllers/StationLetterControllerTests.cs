using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using NSubstitute;
using TGS.Apis.BackendOffice.Controllers;
using TGS.Apis.BackendOffice.Controllers.Messages.StationLetter;
using TGS.Apis.BackendOffice.DataService.Services;
using TGS.Apis.BackendOffice.Services;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Json;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class StationLetterControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Can_Create()
    {
        var commandsMock = Substitute.For<ITgsCommands<StationLetter>>();

        var playerQueriesMock = Substitute.For<ITgsQueries<Player>>();

        var queriesMock = Substitute.For<ITgsQueries<StationLetter>>();

        var stationLetterServiceMock = Substitute.For<IStationLetterService>();

        var uploadServiceMock = Substitute.For<IUploadService>();

        var controller =
            new StationLetterController(queriesMock, playerQueriesMock, commandsMock, uploadServiceMock,
                stationLetterServiceMock)
            { ControllerContext = { HttpContext = HttpContextMock } };

        var players = CreatePlayerFixture()
            .CreateMany<Player>(200)
            .AsQueryable()
            .BuildMock();

        var request = new StationLetterRequest
        {
            BrandId = Guid.NewGuid(),
            AssignedAllPlayers = false,
            Content = "test1",
            Localization = JsonDocument.Parse("{}"),
            Subject = "subject",
            From = DateTimeOffset.Now,
            To = DateTimeOffset.Now.AddDays(1),
            PlayerUsernames = []
        };

        request.PlayerUsernames = [
            .. players
                .Where(p => p.Status == PlayerStatus.Enabled && p.BrandId == request.BrandId)
                .Select(p => p.Username)
        ];

        _ = playerQueriesMock.GetQueries().Returns(players);

        var serializationProvider = new JsonSerializationProvider();

        var jsonOptionsAccessor = new OptionsWrapper<JsonOptions>(new());

        var response = await controller.CreateAsync(
            serializationProvider,
            jsonOptionsAccessor,
            request);

        _ = response.Valid.Should().BeTrue();

        _ = stationLetterServiceMock.Received(1).CreateStationLetterAsync(
            Arg.Is<StationLetter>(s =>
                s.Subject == request.Subject &&
                s.Content == request.Content &&
                s.AssignedAllPlayers == request.AssignedAllPlayers),
            Arg.Is<StationLetterPlayer[]>(x => x.Length == request.PlayerUsernames.Length));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_List(bool showAll)
    {
        var commandsMock = Substitute.For<ITgsCommands<StationLetter>>();

        var playerQueriesMock = Substitute.For<ITgsQueries<Player>>();

        var queriesMock = Substitute.For<ITgsQueries<StationLetter>>();

        var stationLetterServiceMock = Substitute.For<IStationLetterService>();

        var uploadServiceMock = Substitute.For<IUploadService>();

        var controller =
            new StationLetterController(queriesMock, playerQueriesMock, commandsMock, uploadServiceMock,
                stationLetterServiceMock)
            { ControllerContext = { HttpContext = HttpContextMock } };

        var request = new GetStationLettersRequest
        {
            Status = StationLetterStatus.InVerify,
            Page = 1,
            Rows = 10,
            SortBy = "CreatedOn",
            OrderBy = Core.Models.Messages.SortOrder.Descending,
            ShowAll = showAll
        };

        var list = CreateStationLetterFixture()
            .CreateMany<StationLetter>(105)
            .AsQueryable()
            .BuildMock();

        list = list.Where(q => q.Status == request.Status);

        var (skip, take) = GetPage(request);
        var orderedPlayers = list.OrderByDescending(p => p.CreatedOn);

        var expectedPlayers = showAll ? orderedPlayers.ToArray() : orderedPlayers.Skip(skip).Take(take).ToArray();

        _ = queriesMock.GetQueries().Returns(list);

        var response = await controller.GetListAsync(request);

        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            _ = response.Items[i].Id.Should().Be(expectedPlayers[i].Id);
            _ = response.Items[i].Subject.Should().Be(expectedPlayers[i].Subject);
            _ = response.Items[i].Content.Should().Be(expectedPlayers[i].Content);
            _ = response.Items[i].Status.Should().Be(expectedPlayers[i].Status.ToString());
            _ = response.Items[i].Remarks.Should().Be(expectedPlayers[i].Remarks);

            _ = response.Items[i].CreatedByUsername.Should().Be(expectedPlayers[i].CreatedByUsername);
            _ = response.Items[i].VerifiedByUsername.Should().Be(expectedPlayers[i].VerifiedByUsername);
            _ = response.Items[i].CreatedOn.Should().Be(expectedPlayers[i].CreatedOn);
        }

        _ = queriesMock.Received(1).GetQueries();
    }

    [Test]
    public async Task Can_Update()
    {
        var commandsMock = Substitute.For<ITgsCommands<StationLetter>>();

        var playerQueriesMock = Substitute.For<ITgsQueries<Player>>();

        var queriesMock = Substitute.For<ITgsQueries<StationLetter>>();

        var stationLetterServiceMock = Substitute.For<IStationLetterService>();

        var uploadServiceMock = Substitute.For<IUploadService>();

        var controller =
            new StationLetterController(queriesMock, playerQueriesMock, commandsMock, uploadServiceMock,
                stationLetterServiceMock)
            { ControllerContext = { HttpContext = HttpContextMock } };

        var stationLetterId = Guid.NewGuid();

        var request = new StationLetterRequest
        {
            BrandId = Guid.NewGuid(),
            AssignedAllPlayers = false,
            Content = "test1",
            Localization = JsonDocument.Parse("{}"),
            Subject = "subject",
            From = DateTimeOffset.Now,
            To = DateTimeOffset.Now.AddDays(1),
            PlayerUsernames = []
        };
        var stationLetter = CreateStationLetterFixture().Create<StationLetter>();

        _ = queriesMock
            .GetSingleAsync(
                Arg.Any<Expression<Func<StationLetter, bool>>>(),
                Arg.Any<Func<IQueryable<StationLetter>, IQueryable<StationLetter>>>())
            .Returns(Task.FromResult(stationLetter));

        var serializationProvider = new JsonSerializationProvider();

        var jsonOptionsAccessor = new OptionsWrapper<JsonOptions>(new());

        var response = await controller.UpdateAsync(
            serializationProvider,
            jsonOptionsAccessor,
            stationLetterId,
            request);

        _ = response.Valid.Should().BeTrue();

        _ = commandsMock.Received(1).UpdateAsync(Arg.Is<StationLetter>(s =>
            s.Subject == request.Subject &&
            s.Content == request.Content));
    }

    [Test]
    public async Task Can_Update_Not_Found()
    {
        var commandsMock = Substitute.For<ITgsCommands<StationLetter>>();

        var playerQueriesMock = Substitute.For<ITgsQueries<Player>>();

        var queriesMock = Substitute.For<ITgsQueries<StationLetter>>();

        var stationLetterServiceMock = Substitute.For<IStationLetterService>();

        var uploadServiceMock = Substitute.For<IUploadService>();

        var controller =
            new StationLetterController(queriesMock, playerQueriesMock, commandsMock, uploadServiceMock,
                stationLetterServiceMock)
            { ControllerContext = { HttpContext = HttpContextMock } };

        var stationLetterId = Guid.NewGuid();

        var request = new StationLetterRequest
        {
            BrandId = Guid.NewGuid(),
            AssignedAllPlayers = false,
            Content = "test1",
            Localization = JsonDocument.Parse("{}"),
            Subject = "subject",
            From = DateTimeOffset.Now,
            To = DateTimeOffset.Now.AddDays(1),
            PlayerUsernames = []
        };

        _ = queriesMock
            .GetSingleAsync(
                Arg.Any<Expression<Func<StationLetter, bool>>>(),
                Arg.Any<Func<IQueryable<StationLetter>, IQueryable<StationLetter>>>())
            .Returns(Task.FromResult<StationLetter>(default!));

        var serializationProvider = new JsonSerializationProvider();
        var jsonOptionsAccessor = new OptionsWrapper<JsonOptions>(new());

        var response = await controller.UpdateAsync(
            serializationProvider,
            jsonOptionsAccessor,
            stationLetterId,
            request);
        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.StationLetter.NotFound);

        _ = commandsMock.Received(0).UpdateAsync(
            Arg.Is<StationLetter>(s => s.Subject == request.Subject
                && s.Content == request.Content));
    }

    [Test]
    public async Task Can_Upload()
    {
        var commandsMock = Substitute.For<ITgsCommands<StationLetter>>();

        var playerQueriesMock = Substitute.For<ITgsQueries<Player>>();

        var queriesMock = Substitute.For<ITgsQueries<StationLetter>>();

        var stationLetterServiceMock = Substitute.For<IStationLetterService>();

        var uploadServiceMock = Substitute.For<IUploadService>();

        var fileMock = Substitute.For<IFormFile>();

        var controller =
            new StationLetterController(queriesMock, playerQueriesMock, commandsMock, uploadServiceMock,
                stationLetterServiceMock)
            { ControllerContext = { HttpContext = HttpContextMock } };

        var players = CreatePlayerFixture()
            .CreateMany<Player>(100)
            .AsQueryable()
            .BuildMock();

        var playerUsernames = players.Select(p => new UploadPlayerModel { Username = p.Username }).ToList();

        _ = uploadServiceMock.GetUploadedExcelAsync<UploadPlayerModel>(Arg.Any<IFormFile>())
            .Returns(Task.FromResult(playerUsernames));

        _ = playerQueriesMock.GetQueries().Returns(players);

        var expectedPlayers = players
            .ToArray();

        var response = await controller.UploadAsync(fileMock);

        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Players.Length; i++)
        {
            _ = response.Players[i].Username.Should().Be(expectedPlayers.ElementAt(i).Username);
            _ = response.Players[i].Exists.Should().Be(expectedPlayers.ElementAt(i).Status == PlayerStatus.Enabled);
        }
    }

    [TestCase("pass", StationLetterStatus.Published)]
    [TestCase("reject", StationLetterStatus.Rejected)]
    public async Task Can_Verify(string type, StationLetterStatus targetStatus)
    {
        var commandsMock = Substitute.For<ITgsCommands<StationLetter>>();

        var playerQueriesMock = Substitute.For<ITgsQueries<Player>>();

        var queriesMock = Substitute.For<ITgsQueries<StationLetter>>();

        var stationLetterServiceMock = Substitute.For<IStationLetterService>();

        var uploadServiceMock = Substitute.For<IUploadService>();

        var controller =
            new StationLetterController(queriesMock, playerQueriesMock, commandsMock, uploadServiceMock,
                stationLetterServiceMock)
            { ControllerContext = { HttpContext = HttpContextMock } };

        var request = new Fixture().Create<VerifyRequest>();

        var stationLetter = CreateStationLetterFixture().Create<StationLetter>();

        stationLetter.Status = StationLetterStatus.InVerify;

        _ = queriesMock
            .GetSingleAsync(
                Arg.Any<Expression<Func<StationLetter, bool>>>(),
                Arg.Any<Func<IQueryable<StationLetter>, IQueryable<StationLetter>>>())
            .Returns(Task.FromResult(stationLetter));

        var response = await controller.VerifyAsync(stationLetter.Id, type, request);

        _ = response.Valid.Should().BeTrue();

        _ = commandsMock.Received(1).UpdateAsync(Arg.Is<StationLetter>(s =>
            s.Status == targetStatus &&
            s.UpdatedByUsername == Username &&
            s.VerifiedByUsername == Username &&
            s.Remarks == request.Remarks));
    }

    [Test]
    public async Task Cannot_Verify_Not_Found()
    {
        var commandsMock = Substitute.For<ITgsCommands<StationLetter>>();

        var playerQueriesMock = Substitute.For<ITgsQueries<Player>>();

        var queriesMock = Substitute.For<ITgsQueries<StationLetter>>();

        var stationLetterServiceMock = Substitute.For<IStationLetterService>();

        var uploadServiceMock = Substitute.For<IUploadService>();

        var controller =
            new StationLetterController(queriesMock, playerQueriesMock, commandsMock, uploadServiceMock,
                stationLetterServiceMock)
            { ControllerContext = { HttpContext = HttpContextMock } };

        var request = new Fixture().Create<VerifyRequest>();

        var stationLetterId = Guid.NewGuid();

        _ = queriesMock.GetSingleAsync(Arg.Any<Expression<Func<StationLetter, bool>>>(),
                Arg.Any<Func<IQueryable<StationLetter>, IQueryable<StationLetter>>>())
            .Returns(Task.FromResult<StationLetter>(default!));

        var response = await controller.VerifyAsync(stationLetterId, "pass", request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.StationLetter.NotFound);
    }

    [TestCase("pass", StationLetterStatus.Rejected)]
    [TestCase("pass", StationLetterStatus.Published)]
    [TestCase("reject", StationLetterStatus.Rejected)]
    [TestCase("reject", StationLetterStatus.Published)]
    public async Task Cannot_Verify_With_Invalid_Status(string type, StationLetterStatus status)
    {
        var commandsMock = Substitute.For<ITgsCommands<StationLetter>>();

        var playerQueriesMock = Substitute.For<ITgsQueries<Player>>();

        var queriesMock = Substitute.For<ITgsQueries<StationLetter>>();

        var stationLetterServiceMock = Substitute.For<IStationLetterService>();

        var uploadServiceMock = Substitute.For<IUploadService>();

        var controller =
            new StationLetterController(queriesMock, playerQueriesMock, commandsMock, uploadServiceMock,
                stationLetterServiceMock)
            { ControllerContext = { HttpContext = HttpContextMock } };

        var request = new Fixture().Create<VerifyRequest>();

        var stationLetterId = Guid.NewGuid();

        var stationLetter = CreateStationLetterFixture().Create<StationLetter>();

        stationLetter.Status = status;

        _ = queriesMock.GetSingleAsync(Arg.Any<Expression<Func<StationLetter, bool>>>(),
                Arg.Any<Func<IQueryable<StationLetter>, IQueryable<StationLetter>>>())
            .Returns(Task.FromResult(stationLetter));

        var response = await controller.VerifyAsync(stationLetterId, type, request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.StationLetter.InvalidStatus);
    }

    [Test]
    public async Task Cannot_Verify_With_Invalid_Url()
    {
        var commandsMock = Substitute.For<ITgsCommands<StationLetter>>();

        var playerQueriesMock = Substitute.For<ITgsQueries<Player>>();

        var queriesMock = Substitute.For<ITgsQueries<StationLetter>>();

        var stationLetterServiceMock = Substitute.For<IStationLetterService>();

        var uploadServiceMock = Substitute.For<IUploadService>();

        var controller =
            new StationLetterController(queriesMock, playerQueriesMock, commandsMock, uploadServiceMock,
                stationLetterServiceMock)
            { ControllerContext = { HttpContext = HttpContextMock } };

        var request = new Fixture().Create<VerifyRequest>();

        var stationLetterId = Guid.NewGuid();

        var response = await controller.VerifyAsync(stationLetterId, "test", request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.StationLetter.InvalidUrl);
    }

    private static Fixture CreatePlayerFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<Player>(u => u
            .With(s => s.Agent, new Agent())
            .With(s => s.Vip,
                new VipLevel
                {
                    Id = Guid.NewGuid(),
                    Level = 1
                })
            .With(s => s.Brand, new Brand())
            .With(s => s.BetLimit, new BetLimit())
            .With(s => s.Banks, [])
            .With(s => s.PlayerLoginRecords,
                [])
            .With(s => s.Letters,
                [])
            .With(s => s.BrandPaymentPlayers,
                [])
            .With(s => s.SportsBetLimits,
                [])
            .With(s => s.Birthdate, new DateOnly()));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        return fixture;
    }

    private static Fixture CreateStationLetterFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<StationLetter>(u => u
            .With(s => s.Players,
                [])
            .With(s => s.Brand, new Brand())
            .With(s => s.Localization, "{}"));

        return fixture;
    }
}