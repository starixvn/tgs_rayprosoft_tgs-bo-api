using AutoFixture;
using FluentAssertions;
using MediatR;
using NSubstitute;
using TGS.Apis.BackendOffice.Controllers.Brand;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.BrandAllAgent;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class BrandAllAgentControllerTests : BaseTests
{
    [Test]
    public async Task Can_Update_Brand_All_Agent_Switch()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new BrandAllAgentController(mediatorMock);

        var request = new Fixture().Create<UpdateBrandAllAgentSwitchRequest>();

        _ = mediatorMock
            .Send(Arg.Any<UpdateBrandAllAgentSwitchRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { Valid = true });

        var response = await controller.UpdateBrandAllAgentSwitchAsync(request);

        _ = response.Valid.Should().BeTrue();

        _ = mediatorMock.Received(1).Send(Arg.Any<UpdateBrandAllAgentSwitchRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Can_Update_Brand_All_Agent_Settlement_Interval()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new BrandAllAgentController(mediatorMock);

        var request = new Fixture().Create<UpdateBrandAllAgentSettleIntervalRequest>();

        _ = mediatorMock
            .Send(Arg.Any<UpdateBrandAllAgentSettleIntervalRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { Valid = true });

        var response = await controller.UpdateBrandAllAgentSettlementIntervalAsync(request);

        _ = response.Valid.Should().BeTrue();

        _ = mediatorMock.Received(1).Send(Arg.Any<UpdateBrandAllAgentSettleIntervalRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Can_Update_Brand_All_Agent_Game_Type_Switch()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new BrandAllAgentController(mediatorMock);

        var request = new Fixture().Create<UpdateBrandAllAgentGameTypeRequest>();

        _ = mediatorMock
            .Send(Arg.Any<UpdateBrandAllAgentGameTypeRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { Valid = true });

        var response = await controller.UpdateBrandAllAgentGameTypeAsync(request);

        _ = response.Valid.Should().BeTrue();

        _ = mediatorMock.Received(1).Send(Arg.Any<UpdateBrandAllAgentGameTypeRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Can_Update_Brand_All_Agent_Valid_Bet_Conditions()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new BrandAllAgentController(mediatorMock);

        var request = new Fixture().Create<UpdateBrandAllAgentValidBetConditionRequest>();

        _ = mediatorMock
            .Send(Arg.Any<UpdateBrandAllAgentValidBetConditionRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { Valid = true });

        var response = await controller.UpdateBrandAllAgentValidBetConditionsAsync(request);

        _ = response.Valid.Should().BeTrue();

        _ = mediatorMock.Received(1).Send(Arg.Any<UpdateBrandAllAgentValidBetConditionRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Can_Get_Brand_All_Agent()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new BrandAllAgentController(mediatorMock);

        var brandId = Guid.NewGuid();

        _ = mediatorMock
            .Send(Arg.Any<GetBrandAllAgentRequest>(), Arg.Any<CancellationToken>())
            .Returns(new GetBrandAllAgentResponse { Switch = true });

        var response = await controller.GetBrandAllAgentAsync(brandId);

        _ = response.Switch.Should().BeTrue();

        _ = mediatorMock.Received(1).Send(Arg.Any<GetBrandAllAgentRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Can_Get_Brand_Valid_Bet_Conditions()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new BrandAllAgentController(mediatorMock);

        var brandId = Guid.NewGuid();

        _ = mediatorMock
            .CreateStream(Arg.Any<GetBrandAllAgentValidBetSettingRequest>(), Arg.Any<CancellationToken>())
            .Returns(new[]{new ValidBetConditionModel{ Rank = 1 }}.ToAsyncEnumerable());

        var response = await controller.GetBrandAllAgentValidBetConditionAsync(brandId, AllAgentGameType.CardGame).ToArrayAsync();

        Assert.That(response.Length, Is.EqualTo(1));

        _ = mediatorMock.Received(1).CreateStream(Arg.Any<GetBrandAllAgentValidBetSettingRequest>(), Arg.Any<CancellationToken>());
    }

}