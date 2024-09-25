using System.Linq.Expressions;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using TGS.Apis.BackendOffice.Controllers;
using TGS.Apis.BackendOffice.Controllers.Messages;
using TGS.Apis.BackendOffice.Controllers.Messages.Announcement;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Json;
using TGS.Core.Infrastructures.Redis;
using TGS.Core.Models.Enums;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class AnnouncementControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Can_Change_Announcement_Order()
    {
        var commandsMock = Substitute.For<ITgsCommands<Announcement>>();

        var queriesMock = Substitute.For<ITgsQueries<Announcement>>();

        var redisCacheManagerMock = Substitute.For<IRedisCacheManager>();

        var controller = new AnnouncementController(queriesMock, commandsMock, redisCacheManagerMock);

        var list = CreateAnnouncementFixture()
            .CreateMany<Announcement>(105)
            .AsQueryable()
            .BuildMock();

        var request = new ChangeOrderRequest
        {
            Orders =
            [
                new ChangeOrderModel
                {
                    Id = (await list.FirstOrDefaultAsync())!.Id,
                    DisplayOrder = 1
                },
                new ChangeOrderModel
                {
                    Id = (await list.LastOrDefaultAsync())!.Id,
                    DisplayOrder = 2
                }
            ]
        };
        _ = queriesMock.GetQueries()
            .Returns(list);

        var response = await controller.ChangeOrderAsync(request);

        _ = response.Valid.Should().BeTrue();

        await commandsMock.Received(1).BatchUpdateAsync(Arg.Is<Announcement[]>(d =>
            d.Length == request.Orders.Length));
    }

    [TestCase(AnnounceStatus.Effective)]
    [TestCase(AnnounceStatus.Invalid)]
    public async Task Can_Change_Announcement_Status(AnnounceStatus status)
    {
        var commandsMock = Substitute.For<ITgsCommands<Announcement>>();

        var queriesMock = Substitute.For<ITgsQueries<Announcement>>();

        var redisCacheManagerMock = Substitute.For<IRedisCacheManager>();

        var controller = new AnnouncementController(queriesMock, commandsMock, redisCacheManagerMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var announcement = CreateAnnouncementFixture()
            .Create<Announcement>();

        _ = queriesMock.GetSingleAsync(Arg.Any<Expression<Func<Announcement, bool>>>(),
                Arg.Any<Func<IQueryable<Announcement>, IQueryable<Announcement>>>())
            .Returns(Task.FromResult(announcement));

        var response = await controller.ChangeStatusAsync(announcement.Id, status);

        _ = response.Valid.Should().BeTrue();

        _ = await queriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Announcement, bool>>>(),
            Arg.Any<Func<IQueryable<Announcement>, IQueryable<Announcement>>>());

        await commandsMock.Received(1).UpdateAsync(Arg.Is<Announcement>(d =>
            d.Status == status));
    }

    [Test]
    public async Task Can_Create_Announcement()
    {
        var commandsMock = Substitute.For<ITgsCommands<Announcement>>();

        var queriesMock = Substitute.For<ITgsQueries<Announcement>>();

        var redisCacheManagerMock = Substitute.For<IRedisCacheManager>();

        var controller = new AnnouncementController(queriesMock, commandsMock, redisCacheManagerMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var request = new AnnouncementRequest
        {
            BrandId = Guid.NewGuid(),
            Content = "test",
            From = DateTimeOffset.Now,
            To = DateTimeOffset.Now.AddDays(1D),
            Localization = JsonDocument.Parse("{}"),
            Subject = "subject"
        };

        _ = queriesMock
            .ExistsAsync(Arg.Any<Expression<Func<Announcement, bool>>>())
            .Returns(Task.FromResult(false));

        var jsonSerializationProvider = new JsonSerializationProvider();
        var jsonOptionsAccessor = new OptionsWrapper<JsonOptions>(new());

        var response = await controller.CreateAsync(
            jsonSerializationProvider,
            jsonOptionsAccessor,
            request);

        _ = response.Valid.Should().BeTrue();

        await commandsMock.Received(1).CreateAsync(Arg.Is<Announcement>(d =>
            d.Subject == request.Subject &&
            d.Content == request.Content &&
            d.From == request.From &&
            d.To == request.To &&
            d.Status == AnnounceStatus.InVerify));
    }

    [TestCase(true,
        new[]
        {
            AnnounceStatus.Effective,
            AnnounceStatus.Invalid
        })]
    [TestCase(false,
        new[]
        {
            AnnounceStatus.Expired
        })]
    public async Task Can_Get_Announcements(bool showAll, AnnounceStatus[] status)
    {
        var commandsMock = Substitute.For<ITgsCommands<Announcement>>();

        var queriesMock = Substitute.For<ITgsQueries<Announcement>>();

        var redisCacheManagerMock = Substitute.For<IRedisCacheManager>();

        var controller = new AnnouncementController(queriesMock, commandsMock, redisCacheManagerMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var request = new GetAnnouncementsRequest
        {
            Page = 1,
            Rows = 10,
            SortBy = "UpdatedOn",
            OrderBy = Core.Models.Messages.SortOrder.Descending,
            ShowAll = showAll,
            Status = status
        };

        var list = CreateAnnouncementFixture()
            .CreateMany<Announcement>(105)
            .AsQueryable()
            .BuildMock();

        var hasExpiredStatus = request.Status.Contains(AnnounceStatus.Expired);

        var (skip, take) = GetPage(request);

        var orderedRoles = list.Where(hasExpiredStatus, p => DateTimeOffset.Now > p.To)
            .Where(!hasExpiredStatus, p => request.Status.Contains(p.Status) && DateTimeOffset.Now < p.To)
            .Where(!request.Status.Contains(AnnounceStatus.Expired),
                p => request.Status == null || request.Status.Contains(p.Status))
            .Where(x => x.Status != AnnounceStatus.Archived)
            .OrderByDescending(p => p.UpdatedOn);

        var expectedRoles = showAll ? orderedRoles.ToArray() : [.. orderedRoles.Skip(skip).Take(take)];

        _ = queriesMock.GetQueries()
            .Returns(list);

        var response = await controller.GetListAsync(request);
        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            _ = response.Items[i].Id.Should().Be(expectedRoles[i].Id);
            _ = response.Items[i].Subject.Should().Be(expectedRoles[i].Subject);
            _ = response.Items[i].Content.Should().Be(expectedRoles[i].Content);
            _ = response.Items[i].Remarks.Should().Be(expectedRoles[i].Remarks);
            _ = response.Items[i].CreatedByUsername.Should().Be(expectedRoles[i].CreatedByUsername);
            _ = response.Items[i].VerifiedByUsername.Should().Be(expectedRoles[i].VerifiedByUsername);
            _ = response.Items[i].From.Should().Be(expectedRoles[i].From);
            _ = response.Items[i].To.Should().Be(expectedRoles[i].To);
            _ = response.Items[i].UpdatedOn.Should().Be(expectedRoles[i].UpdatedOn);

            _ = response.Items[i].Status.Should().Be(DateTimeOffset.Now > expectedRoles[i].To
                ? AnnounceStatus.Expired.ToString()
                : expectedRoles[i].Status.ToString());
        }

        _ = queriesMock.Received(1).GetQueries();
    }

    [TestCase("invalid", AnnounceStatus.Effective, AnnounceStatus.Invalid)]
    public async Task Can_Invalid_Announcement(
        string type, AnnounceStatus allowedStatus,
        AnnounceStatus verifiedStatus)
    {
        var commandsMock = Substitute.For<ITgsCommands<Announcement>>();

        var queriesMock = Substitute.For<ITgsQueries<Announcement>>();

        var redisCacheManagerMock = Substitute.For<IRedisCacheManager>();

        var controller = new AnnouncementController(queriesMock, commandsMock, redisCacheManagerMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var announcement = CreateAnnouncementFixture().Create<Announcement>();
        announcement.Status = allowedStatus;
        var announcementId = Guid.NewGuid();

        var request = new Fixture().Create<VerifyRequest>();

        _ = queriesMock.GetSingleAsync(Arg.Any<Expression<Func<Announcement, bool>>>(),
                Arg.Any<Func<IQueryable<Announcement>, IQueryable<Announcement>>>())
            .Returns(Task.FromResult(announcement));

        var response = await controller.VerifyAsync(announcementId, type, request);

        _ = response.Valid.Should().BeTrue();

        _ = await queriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Announcement, bool>>>(),
            Arg.Any<Func<IQueryable<Announcement>, IQueryable<Announcement>>>());

        await commandsMock.Received(1).UpdateAsync(Arg.Is<Announcement>(d =>
            d.UpdatedByUsername == "admin.enabled" &&
            d.Status == verifiedStatus));
    }

    [Test]
    public async Task Can_Update_Announcement()
    {
        var commandsMock = Substitute.For<ITgsCommands<Announcement>>();

        var queriesMock = Substitute.For<ITgsQueries<Announcement>>();

        var redisCacheManagerMock = Substitute.For<IRedisCacheManager>();

        var controller = new AnnouncementController(queriesMock, commandsMock, redisCacheManagerMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var announcement = CreateAnnouncementFixture().Create<Announcement>();
        announcement.Status = AnnounceStatus.Invalid;

        var request = new AnnouncementRequest
        {
            BrandId = Guid.NewGuid(),
            Content = "test",
            From = DateTimeOffset.Now,
            To = DateTimeOffset.Now.AddDays(1D),
            Localization = JsonDocument.Parse("{}"),
            Subject = "subject"
        };

        _ = queriesMock
            .GetSingleAsync(
                Arg.Any<Expression<Func<Announcement, bool>>>(),
                Arg.Any<Func<IQueryable<Announcement>, IQueryable<Announcement>>>())
            .Returns(Task.FromResult(announcement));

        _ = queriesMock
            .ExistsAsync(Arg.Any<Expression<Func<Announcement, bool>>>())
            .Returns(Task.FromResult(false));

        var jsonSerializationProvider = new JsonSerializationProvider();
        var jsonOptionsAccessor = new OptionsWrapper<JsonOptions>(new());

        var response = await controller.UpdateAsync(
            jsonSerializationProvider,
            jsonOptionsAccessor,
            announcement.Id,
            request);

        _ = response.Valid.Should().BeTrue();

        _ = await queriesMock.Received(1).GetSingleAsync(
            Arg.Any<Expression<Func<Announcement, bool>>>(),
            Arg.Any<Func<IQueryable<Announcement>, IQueryable<Announcement>>>());

        await commandsMock.Received(1).UpdateAsync(
            Arg.Is<Announcement>(d => d.Subject == request.Subject
                && d.Content == request.Content
                && d.From == request.From
                && d.To == request.To
                && d.Status == AnnounceStatus.InVerify));
    }

    [Test]
    public async Task Can_Update_Announcement_Not_Found()
    {
        var commandsMock = Substitute.For<ITgsCommands<Announcement>>();

        var queriesMock = Substitute.For<ITgsQueries<Announcement>>();

        var redisCacheManagerMock = Substitute.For<IRedisCacheManager>();

        var controller = new AnnouncementController(queriesMock, commandsMock, redisCacheManagerMock);

        var announcement = CreateAnnouncementFixture().Create<Announcement>();
        announcement.Status = AnnounceStatus.InVerify;

        var request = new AnnouncementRequest
        {
            BrandId = Guid.NewGuid(),
            Content = "test",
            From = DateTimeOffset.Now,
            To = DateTimeOffset.Now.AddDays(1D),
            Localization = JsonDocument.Parse("{}"),
            Subject = "subject"
        };

        _ = queriesMock
            .GetSingleAsync(
                Arg.Any<Expression<Func<Announcement, bool>>>(),
                Arg.Any<Func<IQueryable<Announcement>, IQueryable<Announcement>>>())
            .Returns(Task.FromResult<Announcement>(default!));

        _ = queriesMock
            .ExistsAsync(Arg.Any<Expression<Func<Announcement, bool>>>())
            .Returns(Task.FromResult(false));

        var jsonSerializationProvider = new JsonSerializationProvider();
        var jsonOptionsAccessor = new OptionsWrapper<JsonOptions>(new());

        var response = await controller.UpdateAsync(
            jsonSerializationProvider,
            jsonOptionsAccessor,
            announcement.Id,
            request);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Announcement.NotFound);

        _ = await queriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Announcement, bool>>>(),
            Arg.Any<Func<IQueryable<Announcement>, IQueryable<Announcement>>>());

        await commandsMock.Received(0).UpdateAsync(Arg.Is<Announcement>(d =>
            d.Subject == request.Subject &&
            d.Content == request.Content &&
            d.From == request.From &&
            d.To == request.To &&
            d.Status == AnnounceStatus.InVerify));
    }

    [TestCase("pass", AnnounceStatus.InVerify, AnnounceStatus.Effective)]
    [TestCase("reject", AnnounceStatus.InVerify, AnnounceStatus.Rejected)]
    public async Task Can_Verify_Announcement(
        string type, AnnounceStatus allowedStatus,
        AnnounceStatus verifiedStatus)
    {
        var commandsMock = Substitute.For<ITgsCommands<Announcement>>();

        var queriesMock = Substitute.For<ITgsQueries<Announcement>>();

        var redisCacheManagerMock = Substitute.For<IRedisCacheManager>();

        var controller = new AnnouncementController(queriesMock, commandsMock, redisCacheManagerMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var announcement = CreateAnnouncementFixture().Create<Announcement>();
        announcement.Status = allowedStatus;
        var announcementId = Guid.NewGuid();

        var request = new Fixture().Create<VerifyRequest>();

        _ = queriesMock
            .GetSingleAsync(
                Arg.Any<Expression<Func<Announcement, bool>>>(),
                Arg.Any<Func<IQueryable<Announcement>, IQueryable<Announcement>>>())
            .Returns(Task.FromResult(announcement));

        var response = await controller.VerifyAsync(announcementId, type, request);

        _ = response.Valid.Should().BeTrue();

        _ = await queriesMock.Received(1).GetSingleAsync(
            Arg.Any<Expression<Func<Announcement, bool>>>(),
            Arg.Any<Func<IQueryable<Announcement>, IQueryable<Announcement>>>());

        await commandsMock.Received(1).UpdateAsync(
            Arg.Is<Announcement>(d => d.UpdatedByUsername == "admin.enabled"
                && d.Status == verifiedStatus
                && d.Remarks == request.Remarks));
    }

    [Test]
    public async Task Cannot_Change_Announcement_Status_Not_Found()
    {
        var commandsMock = Substitute.For<ITgsCommands<Announcement>>();

        var queriesMock = Substitute.For<ITgsQueries<Announcement>>();

        var redisCacheManagerMock = Substitute.For<IRedisCacheManager>();

        var controller = new AnnouncementController(queriesMock, commandsMock, redisCacheManagerMock);

        _ = queriesMock.GetSingleAsync(Arg.Any<Expression<Func<Announcement, bool>>>(),
                Arg.Any<Func<IQueryable<Announcement>, IQueryable<Announcement>>>())
            .Returns(Task.FromResult<Announcement>(default!));

        var response = await controller.ChangeStatusAsync(Guid.NewGuid(), AnnounceStatus.Invalid);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Announcement.NotFound);
    }

    [Test]
    public async Task Cannot_Update_Announcement_With_Invalid_Status()
    {
        var commandsMock = Substitute.For<ITgsCommands<Announcement>>();

        var queriesMock = Substitute.For<ITgsQueries<Announcement>>();

        var redisCacheManagerMock = Substitute.For<IRedisCacheManager>();

        var controller = new AnnouncementController(queriesMock, commandsMock, redisCacheManagerMock);

        var announcement = CreateAnnouncementFixture().Create<Announcement>();
        announcement.Status = AnnounceStatus.InVerify;

        var request = new AnnouncementRequest
        {
            BrandId = Guid.NewGuid(),
            Content = "test",
            From = DateTimeOffset.Now,
            To = DateTimeOffset.Now.AddDays(1D),
            Localization = JsonDocument.Parse("{}"),
            Subject = "subject"
        };

        _ = queriesMock
            .GetSingleAsync(
                Arg.Any<Expression<Func<Announcement, bool>>>(),
                Arg.Any<Func<IQueryable<Announcement>, IQueryable<Announcement>>>())
            .Returns(Task.FromResult(announcement));

        _ = queriesMock
            .ExistsAsync(Arg.Any<Expression<Func<Announcement, bool>>>())
            .Returns(Task.FromResult(true));

        var jsonSerializationProvider = new JsonSerializationProvider();
        var jsonOptionsAccessor = new OptionsWrapper<JsonOptions>(new());

        var response = await controller.UpdateAsync(
            jsonSerializationProvider,
            jsonOptionsAccessor,
            announcement.Id,
            request);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Announcement.CannotUpdateDueToInvalidStatus);

        _ = await queriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Announcement, bool>>>(),
            Arg.Any<Func<IQueryable<Announcement>, IQueryable<Announcement>>>());

        await commandsMock.Received(0).UpdateAsync(Arg.Is<Announcement>(d =>
            d.Subject == request.Subject &&
            d.Content == request.Content &&
            d.From == request.From &&
            d.To == request.To &&
            d.Status == AnnounceStatus.InVerify));
    }

    [Test]
    public async Task Cannot_Verify_Announcement_Not_Found()
    {
        var commandsMock = Substitute.For<ITgsCommands<Announcement>>();

        var queriesMock = Substitute.For<ITgsQueries<Announcement>>();

        var redisCacheManagerMock = Substitute.For<IRedisCacheManager>();

        var controller = new AnnouncementController(queriesMock, commandsMock, redisCacheManagerMock);

        var announcementId = Guid.NewGuid();

        var request = new Fixture().Create<VerifyRequest>();

        _ = queriesMock.GetSingleAsync(Arg.Any<Expression<Func<Announcement, bool>>>(),
                Arg.Any<Func<IQueryable<Announcement>, IQueryable<Announcement>>>())
            .Returns(Task.FromResult<Announcement>(default!));

        var response = await controller.VerifyAsync(announcementId, "pass", request);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Announcement.NotFound);

        _ = await queriesMock.Received(1).GetSingleAsync(Arg.Any<Expression<Func<Announcement, bool>>>(),
            Arg.Any<Func<IQueryable<Announcement>, IQueryable<Announcement>>>());

        await commandsMock.Received(0).UpdateAsync(Arg.Is<Announcement>(d =>
            d.UpdatedByUsername == "admin.enabled" &&
            d.Remarks == request.Remarks));
    }

    [Test]
    public async Task Cannot_Verify_Announcement_With_Invalid_Path()
    {
        var commandsMock = Substitute.For<ITgsCommands<Announcement>>();

        var queriesMock = Substitute.For<ITgsQueries<Announcement>>();

        var redisCacheManagerMock = Substitute.For<IRedisCacheManager>();

        var controller = new AnnouncementController(queriesMock, commandsMock, redisCacheManagerMock);

        var announcement = CreateAnnouncementFixture().Create<Announcement>();
        var announcementId = Guid.NewGuid();

        var request = new Fixture().Create<VerifyRequest>();

        _ = queriesMock.GetSingleAsync(Arg.Any<Expression<Func<Announcement, bool>>>(),
                Arg.Any<Func<IQueryable<Announcement>, IQueryable<Announcement>>>())
            .Returns(Task.FromResult(announcement));

        var response = await controller.VerifyAsync(announcementId, "pass2", request);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.Announcement.InvalidUrl);
    }

    [TestCase("pass", AnnounceStatus.Effective, "Only announcement in verify can be passed.")]
    [TestCase("pass", AnnounceStatus.Invalid, "Only announcement in verify can be passed.")]
    [TestCase("pass", AnnounceStatus.Expired, "Only announcement in verify can be passed.")]
    [TestCase("pass", AnnounceStatus.Rejected, "Only announcement in verify can be passed.")]
    [TestCase("reject", AnnounceStatus.Effective, "Only announcement in verify can be rejected.")]
    [TestCase("reject", AnnounceStatus.Invalid, "Only announcement in verify can be rejected.")]
    [TestCase("reject", AnnounceStatus.Expired, "Only announcement in verify can be rejected.")]
    [TestCase("reject", AnnounceStatus.Rejected, "Only announcement in verify can be rejected.")]
    [TestCase("invalid", AnnounceStatus.InVerify, "Only effective announcement can be invalided.")]
    [TestCase("invalid", AnnounceStatus.Invalid, "Only effective announcement can be invalided.")]
    [TestCase("invalid", AnnounceStatus.Expired, "Only effective announcement can be invalided.")]
    [TestCase("invalid", AnnounceStatus.Rejected, "Only effective announcement can be invalided.")]
    public async Task Cannot_Verify_Announcement_With_Invalid_Status(
        string type, AnnounceStatus invalidStatus,
        string message)
    {
        var commandsMock = Substitute.For<ITgsCommands<Announcement>>();

        var queriesMock = Substitute.For<ITgsQueries<Announcement>>();

        var redisCacheManagerMock = Substitute.For<IRedisCacheManager>();

        var controller = new AnnouncementController(queriesMock, commandsMock, redisCacheManagerMock);

        var announcementId = Guid.NewGuid();
        var announcement = CreateAnnouncementFixture().Create<Announcement>();
        announcement.Status = invalidStatus;

        var request = new Fixture().Create<VerifyRequest>();

        _ = queriesMock.GetSingleAsync(Arg.Any<Expression<Func<Announcement, bool>>>(),
                Arg.Any<Func<IQueryable<Announcement>, IQueryable<Announcement>>>())
            .Returns(Task.FromResult(announcement));

        var response = await controller.VerifyAsync(announcementId, type, request);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(message);
    }

    private static Fixture CreateAnnouncementFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<Announcement>(u => u
            .With(s => s.Brand, new Brand())
            .With(s => s.Localization, "{}"));

        return fixture;
    }
}