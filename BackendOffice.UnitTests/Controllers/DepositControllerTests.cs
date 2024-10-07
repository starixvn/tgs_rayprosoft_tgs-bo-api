using AutoFixture;
using FluentAssertions;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using TGS.Apis.BackendOffice.Controllers.Messages.Deposit;
using TGS.Apis.BackendOffice.Controllers.Payment;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using Tgs.Promotion.Clients;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Application.Queries.Deposit;
using TGS.Services.BackOffice.Domain;
using Tgs.ServiceUtility;
using PlayerInfo = Tgs.PlayerService.Client.PlayerInfo;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class DepositControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Get_Deposit_List()
    {
        var fakeGetService = Substitute.For<IGetService<DepositFormQuery, DepositForms>>();
        var fakePromotionClient = Substitute.For<IPromotionClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

        var controller = new DepositController { ControllerContext = { HttpContext = HttpContextMock } };

        var brand = CreateBrandModelFixture().Create<BrandModel>();

        var request = new Fixture().Create<DepositFormRequest>();
        var pageRequest = new Fixture().Create<PageableRequest>();
        var agentId = Guid.NewGuid();
        var agentId2 = Guid.NewGuid();
        var agentId3 = Guid.NewGuid();

        var depositForms = new DepositForms
        {
            TotalCount = 3,
            Items = new DepositFormInfo[]
            {
                new() { PlayerCode = "playerCode1" }, new() { PlayerCode = "playerCode2" },
                new() { PlayerCode = "playerCode3" }, new() { PlayerCode = "playerCode4" }
            }
        };

        var playerInfos = new PlayerInfo[]
        {
            new() { PlayerCode = "playerCode1", AgentId = agentId },
            new() { PlayerCode = "playerCode2", AgentId = agentId2 },
            new() { PlayerCode = "playerCode3", AgentId = agentId3 }, new() { PlayerCode = "playerCode4" }
        };

        var agentNames = new AgentNameInfo[]
        {
            new(agentId, "agent1"), new(agentId2, "agent2"), new(agentId3, "agent3")
        };

        _ = fakeGetService.GetAsync(Arg.Any<DepositFormQuery>()).Returns(depositForms);

        _ = fakePlayerServiceClient
            .QueryPlayerInfoListAsync(brand.Id, Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>()).Returns(playerInfos.ToAsyncEnumerable());

        _ = fakeAgentClient.GetAgentNamesAsync(brand.Id).Returns(agentNames.ToAsyncEnumerable());

        var response = await controller.QueryDepositFormsAsync(fakeGetService, fakePromotionClient,
            fakePromotionServiceClient, fakePlayerServiceClient, fakeAgentClient, brand.Id, request, pageRequest);

        _ = response.Valid.Should().BeTrue();

        Assert.That(response.Items.Length, Is.EqualTo(4));

        Assert.That(response.Items[0].AgentName, Is.EqualTo("agent1"));
        Assert.That(response.Items[1].AgentName, Is.EqualTo("agent2"));
        Assert.That(response.Items[2].AgentName, Is.EqualTo("agent3"));
        Assert.That(response.Items[3].AgentName, Is.EqualTo(null));
    }

    private static Fixture CreateBrandModelFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<BrandModel>(u => u
            .With(s => s.GameProviders, [])
            .With(s => s.Languages, [])
            .With(s => s.Currencies, []));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}