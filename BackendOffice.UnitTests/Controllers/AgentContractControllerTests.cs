using AutoFixture;
using FluentAssertions;
using NSubstitute;
using Tgs.Agent.Dividend.Client;
using Tgs.Agent.Dividend.Client.Models;
using TGS.Apis.BackendOffice.Controllers.Agent;
using TGS.Apis.BackendOffice.Controllers.Brand;
using TGS.Apis.BackendOffice.Controllers.Messages.AgentContract;
using TGS.Apis.BackendOffice.Controllers.Messages.BrandContract;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class AgentContractControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Can_Get_Agent_Contracts()
    {
        var fakeAgentContractClient = Substitute.For<IAgentContractClient>();

        var controller = new AgentContractController { ControllerContext = { HttpContext = HttpContextMock } };

        var agentContracts = new Fixture().CreateMany<AgentDividendContract>(100);

        _ = fakeAgentContractClient.GetAgentContractAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(agentContracts.ToAsyncEnumerable());

        var request = new GetAgentContractsRequest
        {
            AgentId = Guid.NewGuid(),
            Page = 1,
            Rows = 15,
            ShowAll = false,
            SortBy = "name"
        };

        var response = await controller.GetAgentContractsAsync(fakeAgentContractClient, request);

        Assert.That(response.Valid, Is.True);
        Assert.That(response.TotalCount, Is.EqualTo(100));
        Assert.That(response.Items.Length, Is.EqualTo(15));

        _ = fakeAgentContractClient.ReceivedWithAnyArgs(1).GetAgentContractAsync(default, default);
    }
}