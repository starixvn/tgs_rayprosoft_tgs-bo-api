using Microsoft.AspNetCore.Http;
using NSubstitute;
using TGS.Services.BackOffice.Application.Models.Agent;
using TGS.Services.BackOffice.Application.Queries.AgentHandlers;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Tests.BackendOffice.UnitTests.Queries;

public class GetAgentsTests
{
    [Test]
    public void Get_Agents()
    {
        var fakeAgentService = Substitute.For<IAgentService>();
        var fakeHttpContextMock = Substitute.For<IHttpContextAccessor>();

        var agentModels = new AgentsModel[] { new() { Id = Guid.NewGuid() } };

        _ = fakeAgentService.GetAgentsAsync(Arg.Any<GetAgentsRequest>()).Returns((agentModels, 1));

        var sut = new GetAgentsQueryHandler(
            fakeAgentService,
            fakeHttpContextMock);

        var request = new GetAgentsRequest();

        _ = sut.Handle(request, default);

        _ = fakeAgentService.ReceivedWithAnyArgs(1).GetAgentsAsync(null!);
    }
}