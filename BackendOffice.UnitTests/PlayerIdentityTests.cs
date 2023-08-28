using System.Linq.Expressions;
using NSubstitute;
using Tgs.Paging;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Commands.PlayerIdentity;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.ApplicationServices.Players;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class PlayerIdentityTests
{
    [Test]
    public async Task Approve_Player_Identity_Form_Should_Success()
    {
        var fakePlayerService = Substitute.For<IPlayerService>();

        var formId = Guid.NewGuid();
        _ = fakePlayerService
            .GetPlayerIdentityFormByIdAsync(formId)
            .Returns(new PlayerIdentityFormInfo
            {
                FormId = formId,
                Status = PlayerIdentityReviewState.WaitingVerify
            });

        var sut = new ReviewPlayerIdentityFormCommandService(fakePlayerService);

        var brandId = Guid.NewGuid();

        var command = new ReviewPlayerIdentityFormCommand(
            brandId,
            formId,
            PlayerIdentityReviewState.Approve,
            "remark",
            "reviewUserId",
            "reviewUserName",
            "realName",
            "idNumber");

        await sut.ExecuteAsync(command);

        _ = fakePlayerService
            .Received(1)
            .SignOffPlayerIdentityFormAsync(
                brandId,
                formId,
                PlayerIdentityReviewState.Approve,
                "reviewUserId",
                "reviewUserName",
                "remark",
                "realName",
                "idNumber");
    }

    [Test]
    public async Task Approve_Player_Identity_Form_Status_Is_Reject_Should_Failed()
    {
        var fakePlayerService = Substitute.For<IPlayerService>();

        var formId = Guid.NewGuid();
        _ = fakePlayerService
            .GetPlayerIdentityFormByIdAsync(formId)
            .Returns(new PlayerIdentityFormInfo
            {
                FormId = formId,
                Status = PlayerIdentityReviewState.Reject
            });

        var sut = new ReviewPlayerIdentityFormCommandService(fakePlayerService);

        var brandId = Guid.NewGuid();

        var command = new ReviewPlayerIdentityFormCommand(
            brandId,
            formId,
            PlayerIdentityReviewState.Approve,
            "remark",
            "reviewUserId",
            "reviewUserName",
            "realName",
            "idNumber");

        await sut.ExecuteAsync(command);

        _ = fakePlayerService
            .Received(0)
            .SignOffPlayerIdentityFormAsync(
                brandId,
                formId,
                PlayerIdentityReviewState.Approve,
                "reviewUserId",
                "reviewUserName",
                "remark",
                "realName",
                "idNumber");
    }

    [Test]
    public async Task Can_Count_Player_Identity_Forms_Query()
    {
        var fakeBackOfficeRepository = Substitute.For<IBackOfficeRepository>();

        var sut = new GetPlayerIdentityAuthenticationListQueryService(fakeBackOfficeRepository);

        var paging = new Paging<PlayerIdentityAuthenticationInfo>
        {
            RowCount = 10,
            Page = 0
        };

        paging.SortByPropertyName(nameof(PlayerIdentityAuthenticationInfo.CreatedOn), false);

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        var query = new GetPlayerIdentityAuthenticationFormsQuery(
            brandId,
            null,
            null,
            null,
            null,
            null,
            null,
            paging);

        _ = fakeBackOfficeRepository.QueryPlayerIdentityAuthenticationsAsync(
            Arg.Is(brandId),
            null,
            null,
            null,
            null,
            null,
            null)
            .Returns(new[]
            {
                new PlayerIdentityAuthenticationInfo(
                    "",
                    "IdNumber",
                    "RealName",
                    "Remark",
                    PlayerIdentityReviewState.WaitingVerify,
                    "Username",
                    DateTimeOffset.UtcNow,
                    Guid.NewGuid(),
                    "PlayerCode"),
                new PlayerIdentityAuthenticationInfo(
                    "",
                    "IdNumber1",
                    "RealName1",
                    "Remark1",
                    PlayerIdentityReviewState.Approve,
                    "Username1",
                    DateTimeOffset.UtcNow,
                    Guid.NewGuid(),
                    "PlayerCode1")
            }.ToAsyncEnumerable());

        var actual = await sut.GetAsync(query);

        Assert.AreEqual(2, actual);
    }

    [Test]
    public async Task Can_Create_Player_Identity_Auth_Form()
    {
        var fakeBrandService = Substitute.For<IBrandService>();
        var fakePlayerService = Substitute.For<IPlayerService>();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var playerCode = "playerCode";

        _ = fakeBrandService.GetBrandAsync(Arg.Any<Expression<Func<Brand, bool>>>())
            .Returns(new Brand()
            {
                Id = brandId,
                EnablePlayerIdentity = true,
                Status = BrandStatus.Active
            });

        var sut = new CreateIdentityAuthenticationFormCommandService(
            fakeBrandService,
            fakePlayerService);

        var command = new CreateIdentityAuthenticationFormCommand(
            playerCode,
            brandId,
            "620dff81110125f95bb9ccbb");

        var actual = await sut.GetAsync(command);

        _ = fakePlayerService
            .Received(1)
            .CreatePlayerAuthenticationFormAsync(
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<string>());

        _ = fakePlayerService
            .Received(1)
            .UpdatePlayerIdentityAsync(Arg.Any<string>(), Arg.Any<Guid>());

        Assert.IsTrue(actual.Valid);
    }

    [Test]
    public async Task Can_Not_Create_Player_Identity_Auth_Form_When_Brand_EnablePlayerIdentity_Is_False()
    {
        var fakeBrandService = Substitute.For<IBrandService>();
        var fakePlayerService = Substitute.For<IPlayerService>();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        _ = fakeBrandService.GetBrandAsync(Arg.Any<Expression<Func<Brand, bool>>>())
            .Returns(new Brand()
            {
                Id = brandId,
                EnablePlayerIdentity = false,
                Status = BrandStatus.Active
            });

        var sut = new CreateIdentityAuthenticationFormCommandService(fakeBrandService, fakePlayerService);

        var command = new CreateIdentityAuthenticationFormCommand(
            "playerCode",
            brandId,
            "620dff81110125f95bb9ccbb");

        var actual = await sut.GetAsync(command);

        Assert.IsFalse(actual.Valid);
    }

    [Test]
    public async Task Can_Query_Player_Identity_Forms()
    {
        var fakeBackOfficeRepository = Substitute.For<IBackOfficeRepository>();

        var sut = new GetPlayerIdentityAuthenticationListQueryService(fakeBackOfficeRepository);

        var paging = new Paging<PlayerIdentityAuthenticationInfo>
        {
            RowCount = 10,
            Page = 0
        };

        paging.SortByPropertyName(nameof(PlayerIdentityAuthenticationInfo.CreatedOn), false);

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        var query = new GetPlayerIdentityAuthenticationFormsQuery(
            brandId,
            null,
            null,
            null,
            null,
            null,
            null,
            paging);

        _ = fakeBackOfficeRepository.QueryPlayerIdentityAuthenticationsAsync(
            Arg.Is(brandId),
            null,
            null,
            null,
            null,
            null,
            null)
            .Returns(new[]
            {
                new PlayerIdentityAuthenticationInfo(
                    "{}",
                    "IdNumber",
                    "RealName",
                    "Remark",
                    PlayerIdentityReviewState.WaitingVerify,
                    "Username",
                    DateTimeOffset.UtcNow,
                    Guid.NewGuid(),
                    "PlayerCode"),
                new PlayerIdentityAuthenticationInfo(
                    "{}",
                    "IdNumber1",
                    "RealName1",
                    "Remark1",
                    PlayerIdentityReviewState.Approve,
                    "Username1",
                    DateTimeOffset.UtcNow,
                    Guid.NewGuid(),
                    "PlayerCode1")
            }.ToAsyncEnumerable());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakeBackOfficeRepository
            .Received(1)
            .QueryPlayerIdentityAuthenticationsAsync(
                Arg.Any<Guid>(),
                null,
                null,
                null,
                null,
                null,
                null);
    }

    [Test]
    public async Task Get_Player_Identity_Form_Should_Success()
    {
        var fakePlayerService = Substitute.For<IPlayerService>();

        _ = fakePlayerService
            .GetPlayerIdentityFormAsync("playerCode")
            .Returns(new PlayerIdentityFormModel
            {
                Id = Guid.NewGuid(),
                Data = "{}"
            });

        var sut = new GetPlayerIdentityFormQueryService(fakePlayerService);

        _ = await sut.GetAsync(new GetPlayerIdentityFormQuery("playerCode"));

        _ = fakePlayerService.Received(1).GetPlayerIdentityFormAsync("playerCode");
    }

    [Test]
    public async Task Reject_Player_Identity_Form_Should_Success()
    {
        var fakePlayerService = Substitute.For<IPlayerService>();

        var formId = Guid.NewGuid();
        _ = fakePlayerService
            .GetPlayerIdentityFormByIdAsync(formId)
            .Returns(new PlayerIdentityFormInfo
            {
                FormId = formId,
                Status = PlayerIdentityReviewState.WaitingVerify
            });

        var sut = new ReviewPlayerIdentityFormCommandService(fakePlayerService);

        var brandId = Guid.NewGuid();

        var command = new ReviewPlayerIdentityFormCommand(
            brandId,
            formId,
            PlayerIdentityReviewState.Reject,
            "remark",
            "reviewUserId",
            "reviewUserName",
            "realName",
            "idNumber");

        await sut.ExecuteAsync(command);

        _ = fakePlayerService
            .Received(1)
            .SignOffPlayerIdentityFormAsync(
                brandId,
                formId,
                PlayerIdentityReviewState.Reject,
                "reviewUserId",
                "reviewUserName",
                "remark",
                "realName",
                "idNumber");
    }
}
