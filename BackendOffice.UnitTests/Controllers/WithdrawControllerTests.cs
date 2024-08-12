using AutoFixture;
using FluentAssertions;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using TGS.Apis.BackendOffice.Controllers.Messages.Withdraw;
using TGS.Apis.BackendOffice.Controllers.Payment;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Application.Queries.Withdraw;
using TGS.Services.BackOffice.Domain;
using Tgs.ServiceUtility;
using PlayerInfo = Tgs.PlayerService.Client.PlayerInfo;
using UserType = Tgs.Payment.Clients.UserType;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class WithdrawControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Get_Withdraw_List()
    {
        var fakeGetService = Substitute.For<IGetService<WithdrawFormQuery, WithdrawForms>>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeAgentClient = Substitute.For<IAgentClient>();

        var controller = new WithdrawController { ControllerContext = { HttpContext = HttpContextMock } };

        var brand = CreateBrandModelFixture().Create<BrandModel>();

        var request = new Fixture().Create<WithdrawFormRequest>();
        var pageRequest = new Fixture().Create<PageableRequest>();
        var agentId = Guid.NewGuid();
        var agentId2 = Guid.NewGuid();
        var agentId3 = Guid.NewGuid();

        var withdrawForms = new WithdrawForms
        {
            TotalCount = 4,
            Items = new WithdrawFormInfo[]
            {
                new() { UserId = "playerCode1", UserType = UserType.Player },
                new() { UserId = "playerCode2", UserType = UserType.Player },
                new() { UserId = agentId.ToString(), UserType = UserType.Agent },
                new() { UserId = agentId2.ToString(), UserType = UserType.Agent }
            }
        };

        var playerInfos = new PlayerInfo[]
        {
            new() { PlayerCode = "playerCode1", AgentId = agentId },
            new() { PlayerCode = "playerCode2" }
        };

        var agentNames = new AgentNameInfo[]
        {
            new(agentId, "agent1"), new(agentId2, "agent2"), new(agentId3, "agent3")
        };

        _ = fakeGetService.GetAsync(Arg.Any<WithdrawFormQuery>()).Returns(withdrawForms);

        _ = fakePlayerServiceClient
            .QueryPlayerInfoListAsync(brand.Id,
                Arg.Is<IEnumerable<string>>(x => x.SequenceEqual(new[]{ "playerCode1", "playerCode2" })),
                Arg.Any<CancellationToken>()).Returns(playerInfos.ToAsyncEnumerable());

        _ = fakeAgentClient.GetAgentNamesAsync(brand.Id).Returns(agentNames.ToAsyncEnumerable());

        var response = await controller.QueryWithdrawFormsAsync(fakeGetService,
            fakePlayerServiceClient, fakeAgentClient, brand.Id, request, pageRequest);

        _ = response.Valid.Should().BeTrue();

        Assert.That(response.Items.Length, Is.EqualTo(4));

        Assert.That(response.Items[0].AgentName, Is.EqualTo("agent1"));
        Assert.That(response.Items[1].AgentName, Is.EqualTo(null));
        Assert.That(response.Items[2].AgentName, Is.EqualTo(null));
        Assert.That(response.Items[3].AgentName, Is.EqualTo(null));
        Assert.That(response.Items[0].UserType, Is.EqualTo(UserType.Player.ToString()));
        Assert.That(response.Items[1].UserType, Is.EqualTo(UserType.Player.ToString()));
        Assert.That(response.Items[2].UserType, Is.EqualTo(UserType.Agent.ToString()));
        Assert.That(response.Items[3].UserType, Is.EqualTo(UserType.Agent.ToString()));
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