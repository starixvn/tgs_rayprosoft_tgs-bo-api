using NSubstitute;
using NUnit.Framework;
using Tgs.StationLetterService.Client;
using TGS.Services.BackOffice.ApplicationServices.StationLetters;

namespace TGS.Tests.BackendOffice.UnitTests;

public class StationLetterTests
{
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
    public async Task 輸入玩家代碼送出站內信()
    {
        var fakeStationLetterClient = Substitute.For<IStationLetterClient>();

        var sut = new SendLetterCommandService(fakeStationLetterClient);

        var command = new SendLetterCommand(
            "GPI",
            new[] { "player1", "player2" },
            "title",
            "aabcc");

        await sut.ExecuteAsync(command);

        _ = fakeStationLetterClient.Received(1)
            .SendStationLetterAsync(Arg.Is(new StationLetterRequest(
                command.BrandCode,
                command.PlayerCodes,
                command.Subject,
                command.Content)));
    }
}
