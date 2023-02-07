using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using TGS.Apis.BackendOffice.Controllers;
using TGS.Apis.BackendOffice.Controllers.Messages.StationLetter;
using TGS.Apis.BackendOffice.DataService.Services;
using TGS.Apis.BackendOffice.Services;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Json;
using TGS.Core.Models.Enums;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class StationLetterControllerTests : BaseTests
{
    private readonly Mock<ITgsCommands<StationLetter>> _commandsMock = new();

    private readonly Mock<ITgsQueries<Player>> _playerQueriesMock = new();

    private readonly Mock<ITgsQueries<StationLetter>> _queriesMock = new();

    private readonly Mock<IStationLetterService> _stationLetterServiceMock = new();

    private readonly Mock<IUploadService> _uploadServiceMock = new();

    private StationLetterController _controller = default!;

    [Test]
    public async Task Can_Create()
    {
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
            PlayerUsernames = Array.Empty<string>()
        };

        request.PlayerUsernames = players
            .Where(p => p.Status == PlayerStatus.Enabled && p.BrandId == request.BrandId)
            .Select(p => p.Username).ToArray();

        _ = _playerQueriesMock.Setup(p => p.GetQueries())
            .Returns(players);
        var serializationProvider = new JsonSerializationProvider();
        var jsonOptionsAccessor = new OptionsWrapper<JsonOptions>(new JsonOptions());

        var response = await _controller.CreateAsync(
            serializationProvider,
            jsonOptionsAccessor,
            request);
        _ = response.Valid.Should().BeTrue();

        _stationLetterServiceMock.Verify(
            p => p.CreateStationLetterAsync(
                It.Is<StationLetter>(s =>
                    s.Subject == request.Subject &&
                    s.Content == request.Content &&
                    s.AssignedAllPlayers == request.AssignedAllPlayers),
                It.Is<StationLetterPlayer[]>(x => x.Length == request.PlayerUsernames.Length)));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_List(bool showAll)
    {
        var request = new GetStationLettersRequest
        {
            Status = StationLetterStatus.InVerify,
            Page = 1,
            Rows = 10,
            SortBy = "CreatedOn",
            OrderBy = SortOrder.Descending,
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

        _ = _queriesMock.Setup(x => x.GetQueries())
            .Returns(list);

        var response = await _controller.GetListAsync(request);
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

        _queriesMock.Verify(p => p.GetQueries());
    }

    [Test]
    public async Task Can_Update()
    {
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
            PlayerUsernames = Array.Empty<string>()
        };
        var stationLetter = CreateStationLetterFixture().Create<StationLetter>();

        _ = _queriesMock
            .Setup(s => s.GetSingleAsync(
                It.IsAny<Expression<Func<StationLetter, bool>>>(),
                It.IsAny<Func<IQueryable<StationLetter>, IQueryable<StationLetter>>>()))
            .Returns(Task.FromResult(stationLetter));

        var serializationProvider = new JsonSerializationProvider();
        var jsonOptionsAccessor = new OptionsWrapper<JsonOptions>(new JsonOptions());

        var response = await _controller.UpdateAsync(
            serializationProvider,
            jsonOptionsAccessor,
            stationLetterId,
            request);
        _ = response.Valid.Should().BeTrue();

        _commandsMock.Verify(p => p.UpdateAsync(It.Is<StationLetter>(s =>
            s.Subject == request.Subject &&
            s.Content == request.Content)));
    }

    [Test]
    public async Task Can_Update_Not_Found()
    {
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
            PlayerUsernames = Array.Empty<string>()
        };

        var queriesMock = new Mock<ITgsQueries<StationLetter>>();
        var playerQueriesMock = new Mock<ITgsQueries<Player>>();
        var commandsMock = new Mock<ITgsCommands<StationLetter>>();
        var uploadServiceMock = new Mock<IUploadService>();
        var stationLetterServiceMock = new Mock<IStationLetterService>();

        _ = queriesMock
            .Setup(s => s.GetSingleAsync(
                It.IsAny<Expression<Func<StationLetter, bool>>>(),
                It.IsAny<Func<IQueryable<StationLetter>, IQueryable<StationLetter>>>()))
            .Returns(Task.FromResult<StationLetter>(default!));

        var sut = new StationLetterController(
            queriesMock.Object,
            playerQueriesMock.Object,
            commandsMock.Object,
            uploadServiceMock.Object,
            stationLetterServiceMock.Object);

        var serializationProvider = new JsonSerializationProvider();
        var jsonOptionsAccessor = new OptionsWrapper<JsonOptions>(new JsonOptions());

        var response = await sut.UpdateAsync(
            serializationProvider,
            jsonOptionsAccessor,
            stationLetterId,
            request);
        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.StationLetter.NotFound);

        commandsMock.Verify(
            p => p.UpdateAsync(
                It.Is<StationLetter>(s => s.Subject == request.Subject
                    && s.Content == request.Content)),
            Times.Never);
    }

    [Test]
    public async Task Can_Upload()
    {
        var players = CreatePlayerFixture()
            .CreateMany<Player>(100)
            .AsQueryable()
            .BuildMock();

        var playerUsernames = players.Select(p => new UploadPlayerModel
        {
            Username = p.Username
        }).ToList();

        _ = _uploadServiceMock.Setup(p => p.GetUploadedExcelAsync<UploadPlayerModel>(It.IsAny<IFormFile>()))
            .Returns(Task.FromResult(playerUsernames));

        _ = _playerQueriesMock.Setup(p => p.GetQueries())
            .Returns(players);

        var expectedPlayers = players
            .ToArray();

        var response = await _controller.UploadAsync(It.IsAny<IFormFile>());
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
        var request = new Fixture().Create<VerifyRequest>();
        var stationLetter = CreateStationLetterFixture().Create<StationLetter>();
        stationLetter.Status = StationLetterStatus.InVerify;

        _ = _queriesMock
            .Setup(s => s.GetSingleAsync(
                It.IsAny<Expression<Func<StationLetter, bool>>>(),
                It.IsAny<Func<IQueryable<StationLetter>, IQueryable<StationLetter>>>()))
            .Returns(Task.FromResult(stationLetter));

        var response = await _controller.VerifyAsync(stationLetter.Id, type, request);
        _ = response.Valid.Should().BeTrue();

        _commandsMock.Verify(p => p.UpdateAsync(It.Is<StationLetter>(s =>
            s.Status == targetStatus &&
            s.UpdatedByUsername == Username &&
            s.VerifiedByUsername == Username &&
            s.Remarks == request.Remarks)));
    }

    [Test]
    public async Task Cannot_Verify_Not_Found()
    {
        var request = new Fixture().Create<VerifyRequest>();
        var stationLetterId = Guid.NewGuid();

        _ = _queriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<StationLetter, bool>>>(),
                It.IsAny<Func<IQueryable<StationLetter>, IQueryable<StationLetter>>>()))
            .Returns(Task.FromResult<StationLetter>(default!));

        var response = await _controller.VerifyAsync(stationLetterId, "pass", request);
        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.StationLetter.NotFound);
    }

    [TestCase("pass", StationLetterStatus.Rejected)]
    [TestCase("pass", StationLetterStatus.Published)]
    [TestCase("reject", StationLetterStatus.Rejected)]
    [TestCase("reject", StationLetterStatus.Published)]
    public async Task Cannot_Verify_With_Invalid_Status(string type, StationLetterStatus status)
    {
        var request = new Fixture().Create<VerifyRequest>();
        var stationLetterId = Guid.NewGuid();
        var stationLetter = CreateStationLetterFixture().Create<StationLetter>();
        stationLetter.Status = status;

        _ = _queriesMock.Setup(s => s.GetSingleAsync(It.IsAny<Expression<Func<StationLetter, bool>>>(),
                It.IsAny<Func<IQueryable<StationLetter>, IQueryable<StationLetter>>>()))
            .Returns(Task.FromResult(stationLetter));

        var response = await _controller.VerifyAsync(stationLetterId, type, request);
        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.StationLetter.InvalidStatus);
    }

    [Test]
    public async Task Cannot_Verify_With_Invalid_Url()
    {
        var request = new Fixture().Create<VerifyRequest>();
        var stationLetterId = Guid.NewGuid();
        var response = await _controller.VerifyAsync(stationLetterId, "test", request);
        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.StationLetter.InvalidUrl);
    }

    [SetUp]
    public override void Setup()
    {
        base.Setup();

        _controller = new(
            _queriesMock.Object,
            _playerQueriesMock.Object,
            _commandsMock.Object,
            _uploadServiceMock.Object,
            _stationLetterServiceMock.Object);
        _controller.ControllerContext.HttpContext = HttpContextMock.Object;
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
            .With(s => s.Banks, Array.Empty<PlayerBankInfo>())
            .With(s => s.PlayerLoginRecords,
                Array.Empty<PlayerLoginRecord>())
            .With(s => s.Letters,
                Array.Empty<StationLetterPlayer>())
            .With(s => s.Wallets,
                new[]
                {
                    new Core.DataServices.Tgs.Models.Wallet
                    {
                        Balance = 0
                    }
                })
            .With(s => s.BrandPaymentPlayers,
                Array.Empty<BrandPaymentPlayer>())
            .With(s => s.SportsBetLimits,
                Array.Empty<PlayerSportsBetLimit>())
            .With(s => s.Birthdate, new DateOnly()));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        return fixture;
    }

    private static Fixture CreateStationLetterFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<StationLetter>(u => u
            .With(s => s.Players,
                Array.Empty<StationLetterPlayer>())
            .With(s => s.Brand, new Brand())
            .With(s => s.Localization, "{}"));

        return fixture;
    }
}
