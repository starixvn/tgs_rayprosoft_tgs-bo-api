using NSubstitute;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Player;
using TGS.Services.BackOffice.Application.Queries.ReportPlayerQueries;
using AutoFixture;
using TGS.Services.BackOffice.Application.Models.ReportPlayer;

namespace TGS.Tests.BackendOffice.UnitTests.Report;

public class PlayerAllAgentReportTests
{
    [Test]
    public async Task Can_Get_Player_All_Agent_Rebates()
    {
        var fakeReportClient = Substitute.For<IReportPlayerAllAgentClient>();

        var sut = new GetPlayerAllAgentRebatesQuery(fakeReportClient);

        var brandId = Guid.NewGuid();

        var request = new GetPlayerAllAgentRebatesReportRequest() { BrandId = brandId };

        var fixture = new Fixture();

        var playerAllAgentRebates = fixture.CreateMany<PlayerAllAgentRebateModel>(10).ToArray();

        _ = fakeReportClient.GetPlayerAllAgentRebatesAsync(Arg.Any<GetPlayerAllAgentRebatesRequest>())
            .Returns(playerAllAgentRebates.ToAsyncEnumerable());

        var response = await sut.QueryAsync(request, default).ToArrayAsync();

        Assert.That(response.Length, Is.EqualTo(10));

        _ = fakeReportClient.Received(1)
            .GetPlayerAllAgentRebatesAsync(Arg.Any<GetPlayerAllAgentRebatesRequest>());
    }

    [Test]
    public async Task Can_Get_Player_All_Agent_Details()
    {
        var fakeReportClient = Substitute.For<IReportPlayerAllAgentClient>();

        var sut = new GetPlayerAllAgentRebatesDetailQuery(fakeReportClient);

        var brandId = Guid.NewGuid();

        var request = new GetPlayerAllAgentRebatesDetailsReportRequest() { BrandId = brandId };

        var fixture = new Fixture();

        var playerAllAgentRebateDetails = fixture.CreateMany<PlayerAllAgentRebateDetailModel>(10).ToArray();

        _ = fakeReportClient.GetPlayerAllAgentRebateDetailAsync(Arg.Any<GetPlayerAllAgentRebatesDetailsRequest>())
            .Returns(playerAllAgentRebateDetails.ToAsyncEnumerable());

        var response = await sut.QueryAsync(request, default).ToArrayAsync();

        Assert.That(response.Length, Is.EqualTo(10));

        _ = fakeReportClient.Received(1)
            .GetPlayerAllAgentRebateDetailAsync(Arg.Any<GetPlayerAllAgentRebatesDetailsRequest>());
    }

    [Test]
    public async Task Can_Get_Player_All_Agent_Detail_Game_Types()
    {
        var fakeReportClient = Substitute.For<IReportPlayerAllAgentClient>();

        var sut = new GetPlayerAllAgentRebateDetailGameTypesQuery(fakeReportClient);

        var brandId = Guid.NewGuid();

        var request = new GetPlayerAllAgentRebateDetailGameTypesReportRequest() { BrandId = brandId };

        var fixture = new Fixture();

        var playerAllAgentRebateDetailGameTypes = fixture.CreateMany<PlayerAllAgentRebateDetailGameTypeModel>(10).ToArray();

        _ = fakeReportClient.GetPlayerAllAgentRebateDetailGameTypesAsync(Arg.Any<GetPlayerAllAgentRebateDetailGameTypesRequest>())
            .Returns(playerAllAgentRebateDetailGameTypes.ToAsyncEnumerable());

        var response = await sut.QueryAsync(request, default).ToArrayAsync();

        Assert.That(response.Length, Is.EqualTo(10));

        _ = fakeReportClient.Received(1)
            .GetPlayerAllAgentRebateDetailGameTypesAsync(Arg.Any<GetPlayerAllAgentRebateDetailGameTypesRequest>());
    }

    [Test]
    public async Task Can_Get_Player_All_Agent_Detail_Total_Amount()
    {
        var fakeReportClient = Substitute.For<IReportPlayerAllAgentClient>();

        var sut = new GetPlayerAllAgentRebateTotalAmountQuery(fakeReportClient);

        var brandId = Guid.NewGuid();

        var request = new GetPlayerAllAgentRebateTotalAmountReportRequest() { BrandId = brandId };

        var fixture = new Fixture();

        var playerAllAgentRebateDetailGameType = fixture.Create<PlayerAllAgentRebateTotalAmountModel>();

        _ = fakeReportClient.GetPlayerAllAgentRebatesTotalAmountAsync(Arg.Any<GetPlayerAllAgentRebateTotalAmountRequest>())
            .Returns(playerAllAgentRebateDetailGameType);

        var response = await sut.GetAsync(request, default);

        Assert.That(response.ValidBet, Is.EqualTo(playerAllAgentRebateDetailGameType.ValidBet));
        Assert.That(response.Rebate, Is.EqualTo(playerAllAgentRebateDetailGameType.Rebate));
        Assert.That(response.TotalCount, Is.EqualTo(playerAllAgentRebateDetailGameType.TotalCount));

        _ = fakeReportClient.Received(1)
            .GetPlayerAllAgentRebatesTotalAmountAsync(Arg.Any<GetPlayerAllAgentRebateTotalAmountRequest>());
    }
}