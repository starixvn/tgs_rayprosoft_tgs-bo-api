using NSubstitute;
using NUnit.Framework.Legacy;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.Agent.Client.Models.Enums;
using TGS.Services.BackOffice.ApplicationServices.Agents;

namespace TGS.Tests.BackendOffice.UnitTests;

public class AgentPasswordTests
{
    [Test]
    public async Task 設置代理密碼()
    {
        var fakeAgentClient = Substitute.For<IAgentClient>();

        var sut = new SetAgentPasswordCommandService(fakeAgentClient);

        var agentId = Guid.NewGuid();
        var password = "newPassword";
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
                Guid.NewGuid(),
                Guid.NewGuid(),
                null, 
                false,
                true));

        var actual = await sut.GetAsync(new SetAgentPasswordCommand(agentId, password, modifyUserId, modifyUsername));

        _ = fakeAgentClient
            .Received(1)
            .SetAgentPasswordAsync(Arg.Is(new SetAgentPasswordRequest(agentId, password, modifyUserId)));

        var auditRequest = new AddAgentAuditLogRequest(agentId, AgentOperation.Password, modifyUsername);

        _ = fakeAgentClient
            .Received(1)
            .AddAgentAuditLogAsync(Arg.Is(auditRequest));

        ClassicAssert.True(actual.Valid);
    }
}
