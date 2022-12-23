using System;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Services.BackOffice.Application.Commands.BrandLiveChat;
using TGS.Services.BackOffice.Application.Queries.BrandLiveChat;
using CoreModels = TGS.Core.DataServices.Tgs.Models;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class BrandLiveChatTests
{
    [Test]
    public async Task Create_Brand_Live_Chat_Should_Success()
    {
        var fakeTgsCommand = Substitute.For<ITgsCommands<CoreModels.BrandLiveChat>>();

        var sut = new CreateBrandLiveChatCommandService(fakeTgsCommand);

        var command = new CreateBrandLiveChatCommand();
        await sut.ExecuteAsync(command);

        _ = fakeTgsCommand
            .Received(1)
            .CreateAsync(Arg.Any<CoreModels.BrandLiveChat>());
    }

    [Test]
    public async Task Delete_Brand_Live_Chat_Should_Success()
    {
        var fakeTgsQueries = Substitute.For<ITgsQueries<CoreModels.BrandLiveChat>>();
        var fakeTgsCommand = Substitute.For<ITgsCommands<CoreModels.BrandLiveChat>>();

        var sut = new DeleteBrandLiveChatCommandService(fakeTgsQueries, fakeTgsCommand);

        var command = new DeleteBrandLiveChatCommand(Guid.NewGuid());
        await sut.ExecuteAsync(command);

        _ = fakeTgsCommand
            .Received(1)
            .DeleteAsync(Arg.Any<CoreModels.BrandLiveChat>());
    }

    [Test]
    public async Task Get_Brand_Live_Chat_Should_Success()
    {
        var fakeTgsQueries = Substitute.For<ITgsQueries<CoreModels.BrandLiveChat>>();

        var sut = new GetBrandLiveChatQueryService(fakeTgsQueries);

        var brandId = Guid.NewGuid();
        var query = new BrandLiveChatQuery(
            BrandId: brandId);

        var brandLiveChat = new CoreModels.BrandLiveChat()
        {
            BrandId = brandId,
            ChatName = "LiveChat",
            ChatSetting = "{\"url\": \"https://www.google.com/\" }"
        };

        _ = fakeTgsQueries
            .GetSingleAsync(Arg.Any<Expression<Func<CoreModels.BrandLiveChat, bool>>>())
            .Returns(brandLiveChat);

        var info = await sut.GetAsync(query);

        Assert.IsTrue(info.ChatSetting != null);
    }

    [Test]
    public async Task Update_Brand_Live_Chat_Should_Success()
    {
        var fakeTgsQueries = Substitute.For<ITgsQueries<CoreModels.BrandLiveChat>>();
        var fakeTgsCommand = Substitute.For<ITgsCommands<CoreModels.BrandLiveChat>>();

        _ = fakeTgsQueries
            .GetSingleAsync(Arg.Any<Expression<Func<CoreModels.BrandLiveChat, bool>>>())
            .Returns(new CoreModels.BrandLiveChat());

        var sut = new UpdateBrandLiveChatCommandService(fakeTgsQueries, fakeTgsCommand);

        var command = new UpdateBrandLiveChatCommand()
        {
            BrandId = Guid.NewGuid(),
            Name = "LiveChat",
            Setting = JsonDocument.Parse("{}")
        };

        await sut.ExecuteAsync(command);

        _ = fakeTgsCommand
            .Received(1)
            .UpdateAsync(Arg.Any<CoreModels.BrandLiveChat>());
    }
}
