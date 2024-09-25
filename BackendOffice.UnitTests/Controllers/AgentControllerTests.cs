using MediatR;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using TGS.Apis.BackendOffice.Controllers.Agent;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[Parallelizable]
public class AgentControllerTests : BaseTests
{
    [Test]
    public async Task Clear_Negative_Commission()
    {
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeMediator = Substitute.For<IMediator>();

        var sut = new AgentController(fakeMediator)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var agentId = Guid.NewGuid();

        _ = await sut.ClearUnDistributedCommissionAsync(fakeAgentClient, agentId);

        await fakeAgentClient
            .Received(1)
            .ClearAgentNegativeCommissionAsync(Arg.Is<ClearAgentNegativeCommissionRequest>(x
                => agentId == x.AgentId &&
                Guid.Parse("e4ecfa98-b373-4123-87e0-cb9e883d0691") == x.ClearByUserId));
    }

    [SetUp]
    public override void Setup() => base.Setup();
}
