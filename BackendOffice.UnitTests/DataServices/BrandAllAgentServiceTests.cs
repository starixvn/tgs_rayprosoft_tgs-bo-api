using AutoFixture;
using Microsoft.AspNetCore.Http;
using Tgs.Player.All.Agent.Client;
using Tgs.Player.All.Agent.Client.Models;
using TGS.Services.BackOffice.Application.Commands.BrandAllAgentHandlers;
using TGS.Services.BackOffice.Application.Models.BrandAllAgent;
using TGS.Services.BackOffice.Application.Queries.BrandAllAgentHandlers;
using TGS.Tests.BackendOffice.UnitTests.Controllers;
using GetBrandAllAgentRequest = TGS.Services.BackOffice.Application.Models.BrandAllAgent.GetBrandAllAgentRequest;
using ValidBetConditionModel = Tgs.Player.All.Agent.Client.Models.ValidBetConditionModel;

namespace TGS.Tests.BackendOffice.UnitTests.DataServices;

[TestFixture]
[Parallelizable]
public class BrandAllAgentServiceTests : BaseTests
{
    [Test]
    public void Can_Update_Brand_All_Agent_Game_Type_Switch()
    {
        var fakeBrandAllAgentClient = Substitute.For<IBrandAllAgentClient>();
        var fakeHttpContextAccessor = Substitute.For<IHttpContextAccessor>();

        var sut = new UpdateBrandAllAgentGameTypeGameTypeSwitchHandler(fakeBrandAllAgentClient,
            fakeHttpContextAccessor);

        var request = new Fixture().Create<UpdateBrandAllAgentGameTypeRequest>();

        _ = sut.Handle(request, default);

        _ = fakeBrandAllAgentClient.Received(1).UpdateGameTypeSwitchAsync(Arg.Any<UpdateGameTypeSwitchRequest>());
    }

    [Test]
    public void Can_Update_Brand_All_Agent_Switch()
    {
        var fakeBrandAllAgentClient = Substitute.For<IBrandAllAgentClient>();
        var fakeHttpContextAccessor = Substitute.For<IHttpContextAccessor>();

        var sut = new UpdateBrandAllAgentSwitchHandler(fakeBrandAllAgentClient, fakeHttpContextAccessor);

        var request = new Fixture().Create<UpdateBrandAllAgentSwitchRequest>();

        _ = sut.Handle(request, default);

        _ = fakeBrandAllAgentClient.Received(1).UpdateSwitchAsync(Arg.Any<UpdateSwitchRequest>());
    }

    [Test]
    public void Can_Update_Brand_All_Agent_Settlement_Interval()
    {
        var fakeBrandAllAgentClient = Substitute.For<IBrandAllAgentClient>();
        var fakeHttpContextAccessor = Substitute.For<IHttpContextAccessor>();

        var sut = new UpdateBrandAllAgentSettleIntervalHandler(fakeBrandAllAgentClient, fakeHttpContextAccessor);

        var request = new Fixture().Create<UpdateBrandAllAgentSettleIntervalRequest>();

        _ = sut.Handle(request, default);

        _ = fakeBrandAllAgentClient.Received(1)
            .UpdateSettlementIntervalAsync(Arg.Any<UpdateSettlementIntervalRequest>());
    }

    [Test]
    public void Can_Update_Brand_All_Agent_Conditions()
    {
        var fakeBrandAllAgentClient = Substitute.For<IBrandAllAgentClient>();
        var fakeHttpContextAccessor = Substitute.For<IHttpContextAccessor>();

        var sut = new UpdateBrandAllAgentValidBetConditionHandler(fakeBrandAllAgentClient, fakeHttpContextAccessor);

        var request = new Fixture().Create<UpdateBrandAllAgentValidBetConditionRequest>();

        _ = sut.Handle(request, default);

        _ = fakeBrandAllAgentClient.Received(1).UpdateValidBetConditionAsync(Arg.Any<UpdateValidBetConditionRequest>());
    }

    [Test]
    public async Task Can_Get_Brand_All_Agent()
    {
        var fakeBrandAllAgentClient = Substitute.For<IBrandAllAgentClient>();
        var fakeHttpContextAccessor = Substitute.For<IHttpContextAccessor>();

        var sut = new GetBrandAllAgentQueryHandler(fakeBrandAllAgentClient, fakeHttpContextAccessor);

        var request = new Fixture().Create<GetBrandAllAgentRequest>();

        _ = fakeBrandAllAgentClient
            .GetBrandAllAgentAsync(Arg.Any<Tgs.Player.All.Agent.Client.Models.GetBrandAllAgentRequest>())
            .Returns(new BrandAllAgent(true, SettlementIntervalType.Daily));

        _ = fakeBrandAllAgentClient.GetGameTypesAsync(Arg.Any<GetGameTypesRequest>())
            .Returns(new[] { new GameTypeModel { Switch = true, HasValidBet = true } }.ToAsyncEnumerable());

        var response = await sut.Handle(request, default);

        Assert.That(response.Switch, Is.True);
        Assert.That(response.SettlementIntervalType, Is.EqualTo(Core.Models.Enums.SettlementIntervalType.Daily));
        Assert.That(response.BrandAllAgentGameTypes.Length, Is.EqualTo(1));

        _ = fakeBrandAllAgentClient.Received(1)
            .GetBrandAllAgentAsync(
                Arg.Any<Tgs.Player.All.Agent.Client.Models.GetBrandAllAgentRequest>());

        _ = fakeBrandAllAgentClient.Received(1)
            .GetGameTypesAsync(
                Arg.Any<GetGameTypesRequest>());
    }

    [Test]
    public async Task Can_Get_Brand_All_Agent_Conditions()
    {
        var fakeBrandAllAgentClient = Substitute.For<IBrandAllAgentClient>();

        var sut = new GetBrandAllAgentValidBetSettingQueryHandler(fakeBrandAllAgentClient);

        var request = new Fixture().Create<GetBrandAllAgentValidBetSettingRequest>();

        _ = fakeBrandAllAgentClient.GetValidBetConditionsAsync(Arg.Any<GetValidBetConditionsRequest>())
            .Returns(new[]
            {
                new ValidBetConditionModel { Rank = 1, ActiveNumber = 1, NetProfit = 100, RebatePercentage = 10 }
            }.ToAsyncEnumerable());

        var response = await sut.Handle(request, default).ToArrayAsync();

        Assert.That(response.Length, Is.EqualTo(1));

        _ = fakeBrandAllAgentClient.Received(1)
            .GetValidBetConditionsAsync(
                Arg.Any<GetValidBetConditionsRequest>());
    }
}