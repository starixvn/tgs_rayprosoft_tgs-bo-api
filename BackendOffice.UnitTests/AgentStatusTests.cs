using NSubstitute;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.Agent.Client.Models.Enums;
using TGS.Services.BackOffice.ApplicationServices.Agents;

namespace TGS.Tests.BackendOffice.UnitTests;

public class AgentStatusTests
{
    [Test]
    public async Task 變更代理狀態()
    {
        var fakeAgentClient = Substitute.For<IAgentClient>();

        var sut = new SetAgentStatusCommandService(fakeAgentClient);

        var agentId = Guid.NewGuid();
        var status = AgentStatus.InBlacklist;
        var modifyUserId = "6a379883-d8d2-4ffb-3d05-08d9262ce1a5";
        var modifyUsername = "username";

        _ = fakeAgentClient.GetAgentInfoAsync(Arg.Is(new GetAgentInfoRequest(agentId)))
            .Returns(new AgentInfoModel(
                agentId,
                "agentUsername",
                null,
                null,
                null,
                null,
                null,
                "jhmdpi",
                null,
                DateTimeOffset.UtcNow.AddMonths(-1),
                AgentStatus.Enabled,
                AgentType.Cash,
                AgentLevelType.Chief,
                0m,
                0,
                0m,
                Guid.NewGuid()));

        var actual = await sut.GetAsync(new SetAgentStatusCommand(agentId, status, modifyUserId, modifyUsername));

        _ = fakeAgentClient
            .Received(1)
            .SetAgentStatusAsync(Arg.Is(agentId), Arg.Is(status), Arg.Is(modifyUserId));

        _ = fakeAgentClient
            .Received(1)
            .AddAgentAuditLogAsync(Arg.Is(new AddAgentAuditLogRequest(agentId, AgentOperation.Info, modifyUsername)));

        Assert.True(actual.Valid);
    }
}
