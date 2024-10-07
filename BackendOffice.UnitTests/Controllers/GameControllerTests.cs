using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using TGS.Apis.BackendOffice.Controllers.GameProvider;
using TGS.Apis.BackendOffice.Services;
using TGS.Services.BackOffice.Application.Queries.Game;
using Tgs.ServiceUtility;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class GameControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Upload_Game_Localization()
    {
        var mediatorMock = Substitute.For<IMediator>();
        var uploadServiceMock = Substitute.For<IUploadService>();
        var getServiceMock = Substitute.For<IGetService<UpdateGameLocalizationQuery, UpdateGameLocationInfo>>();
        var fileMock = Substitute.For<IFormFile>();

        var controller = new GameController(mediatorMock, uploadServiceMock)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var game1 = new Dictionary<string, string>
        {
            { "Id", "7200001" },
            { "Code", "gt_pt_bal" },
            { "en-US", "Baccarat" },
            { "ja-JP", "" },
            { "zh-CN", "百家乐" },
            { "zh-TW", "百家乐" }
        };

        var game2 = new Dictionary<string, string>
        {
            { "Id", "7200092" },
            { "Code", "gt_pt_chili_eruption_thundershots" },
            { "en-US", "Chili Eruption Thundershots" },
            { "ja-JP", null },
            { "zh-CN", "辣椒喷发：雷霆之弹" },
            { "zh-TW", "辣椒喷发：雷霆之弹" }
        };

        var game3 = new Dictionary<string, string>
        {
            { "Id", "7200107" },
            { "Code", "gt_pt_buffalo_blitz_live_slots" },
            { "en-US", "\t\r\nBuffalo Blitz Live Slots" },
            { "ja-JP", "" },
            { "zh-CN", "真人老虎机水牛闪电战" },
            { "zh-TW", "真人老虎机水牛闪电战" }
        };

        var list = new List<Dictionary<string, string>> { game1, game2, game3 };

        _ = uploadServiceMock.GetUploadedExcelAsync<Dictionary<string, string>>(Arg.Any<IFormFile>()).Returns(list);
        _ = getServiceMock.GetAsync(Arg.Any<UpdateGameLocalizationQuery>())
            .Returns(new UpdateGameLocationInfo { DiffGameIds = [] });

        var response = await controller.UploadGamesLocalizationAsync(fileMock, getServiceMock);

        _ = response.Valid.Should().BeTrue();

        _ = getServiceMock.Received(1).GetAsync(Arg.Any<UpdateGameLocalizationQuery>(), Arg.Any<CancellationToken>());
    }
}