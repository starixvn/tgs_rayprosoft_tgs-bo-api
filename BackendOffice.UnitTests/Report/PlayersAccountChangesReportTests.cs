using NSubstitute;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Player;
using TGS.Services.BackOffice.Application.Queries.ReportPlayerQueries;
using AutoFixture;
using TGS.Services.BackOffice.Application.Models.ReportPlayer;

namespace TGS.Tests.BackendOffice.UnitTests.Report;

public class PlayersAccountChangesReportTests
{
    [Test]
    public async Task Can_Get_Player_All_Agent_Rebates()
    {
        var fakeReportClient = Substitute.For<IReportClient>();

        var sut = new GetPlayersAccountChangesQuery(fakeReportClient);

        var brandId = Guid.NewGuid();

        var request = new GetPlayersAccountChangesReportRequest() { BrandId = brandId };

        var fixture = new Fixture();

        var playerAllAgentRebates = fixture.CreateMany<PlayersAccountChangeModel>(10).ToArray();

        _ = fakeReportClient.GetMultiplePlayersAccountChangesAsync(Arg.Any<GetPlayersAccountChangesRequest>())
            .Returns(playerAllAgentRebates.ToAsyncEnumerable());

        _ = fakeReportClient.GetMultiplePlayersAccountChangesTotalAmountAsync(Arg.Any<GetPlayersAccountChangesTotalAmountRequest>())
            .Returns(new PlayersAccountChangesTotalAmountModel
            {
                TotalCount = 10,
                Amount = 10,
                BeforeBalance = 10,
                AfterBalance = 10
            });

        var response = await sut.GetAsync(request, default);

        Assert.That(response.Items.Count(), Is.EqualTo(10));

        _ = fakeReportClient.Received(1)
            .GetMultiplePlayersAccountChangesAsync(Arg.Any<GetPlayersAccountChangesRequest>());
        _ = fakeReportClient.Received(1)
            .GetMultiplePlayersAccountChangesTotalAmountAsync(Arg.Any<GetPlayersAccountChangesTotalAmountRequest>());
    }
}