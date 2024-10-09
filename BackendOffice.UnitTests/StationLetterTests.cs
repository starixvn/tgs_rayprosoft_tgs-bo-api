using NSubstitute;
using Tgs.StationLetterService.Client;
using TGS.Services.BackOffice.ApplicationServices.StationLetters;

namespace TGS.Tests.BackendOffice.UnitTests;

public class StationLetterTests
{
    [Test]
    public async Task 回覆玩家信件()
    {
        var fakeStationLetterClient = Substitute.For<IStationLetterClient>();

        var sut = new ReplyLetterCommandService(fakeStationLetterClient);

        var command = new ReplyLetterCommand(
            "GPI",
            "test1",
            Guid.NewGuid(),
            "reply",
            "operator");

        await sut.ExecuteAsync(command);

        _ = fakeStationLetterClient.Received(1)
            .ReplyStationLetterAsync(
                Arg.Is(command.LetterId),
                Arg.Is(new ReplyStationLetterRequest(
                    command.BrandCode,
                    command.PlayerCode,
                    command.Content,
                    command.Operator)));
    }

    [Test]
    public async Task 取得Brand收到的站內信()
    {
        var fakeStationLetterClient = Substitute.For<IStationLetterClient>();

        var sut = new GetBrandLettersQueryService(fakeStationLetterClient);

        var query = new GetBrandLettersQuery("GPI");

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakeStationLetterClient.Received(1)
            .ListStationLettersAsync(Arg.Is(query.BrandCode));
    }

    [Test]
    public async Task 取得站內信資料()
    {
        var fakeStationLetterClient = Substitute.For<IStationLetterClient>();

        var sut = new GetLetterByIdQueryService(fakeStationLetterClient);

        var query = new GetLetterByIdQuery(Guid.NewGuid());

        _ = await sut.GetAsync(query);

        _ = fakeStationLetterClient.Received(1)
            .GetStationLetterAsync(Arg.Is(query.LetterId));
    }

    [Test]
    public async Task 取得寄件內容()
    {
        var fakeStationLetterClient = Substitute.For<IStationLetterClient>();

        var sut = new GetDeliveryLetterByIdQueryService(fakeStationLetterClient);

        var query = new GetDeliveryLetterByIdQuery(Guid.NewGuid());

        _ = await sut.GetAsync(query);

        _ = fakeStationLetterClient.Received(1)
            .GetDeliveryLetterAsync(Arg.Is(query.DeliveryId));
    }

    [Test]
    public async Task 取得寄件匣列表()
    {
        var fakeStationLetterClient = Substitute.For<IStationLetterClient>();

        var sut = new GetDeliveryLettersQueryService(fakeStationLetterClient);

        var query = new GetDeliveryLettersQuery("GPI");

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakeStationLetterClient.Received(1)
            .ListDeliverLettersAsync(Arg.Is(query.BrandCode));
    }

    [Test]
    public async Task 送出站內信給全部玩家()
    {
        var fakeStationLetterClient = Substitute.For<IStationLetterClient>();

        var sut = new SendAllPlayersLetterCommandService(fakeStationLetterClient);

        var command = new SendAllPlayersLetterCommand(
            "GPI",
            ["test1"],
            "title",
            "aabbcc",
            "operator");

        await sut.ExecuteAsync(command);

        _ = fakeStationLetterClient.Received(1)
            .SendBrandAllPlayersStationLetterAsync(Arg.Is(new BrandAllPlayersStationLetterRequest(
                command.BrandCode,
                command.PlayerCodes,
                command.Subject,
                command.Content,
                command.Operator)));
    }

    [Test]
    public async Task 輸入玩家代碼送出站內信()
    {
        var fakeStationLetterClient = Substitute.For<IStationLetterClient>();

        var sut = new SendLetterCommandService(fakeStationLetterClient);

        var playerCodes = new[] { "player1", "player2" };
        var command = new SendLetterCommand(
            "GPI",
            playerCodes,
            "title",
            "aabcc",
            "operator");

        await sut.ExecuteAsync(command);

        _ = fakeStationLetterClient.Received(1)
            .SendStationLetterAsync(Arg.Is(new StationLetterRequest(
                command.BrandCode,
                command.PlayerCodes,
                command.Subject,
                command.Content,
                command.Operator)));
    }
}
