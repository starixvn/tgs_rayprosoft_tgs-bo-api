using NSubstitute;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.Agent.Client.Models.Enums;
using TGS.Services.BackOffice.ApplicationServices.Agents;

namespace TGS.Tests.BackendOffice.UnitTests;

public class AgentPayPasswordTests
{
    [Test]
    public async Task 更新代理提款密碼()
    {
        var fakeAgentClient = Substitute.For<IAgentClient>();

        var sut = new UpdateAgentPayPasswordCommandService(fakeAgentClient);

        var agentId = Guid.NewGuid();
        var payPassword = "newPassword";
        var editUsername = "editUsername";

        await sut.ExecuteAsync(new UpdateAgentPayPasswordCommand(agentId, payPassword, editUsername));

        _ = fakeAgentClient
            .Received(1)
            .SetAgentPayPasswordAsync(Arg.Is(new SetAgentPayPasswordRequest(agentId, payPassword)));

        _ = fakeAgentClient
            .Received(1)
            .AddAgentAuditLogAsync(Arg.Is(new AddAgentAuditLogRequest(agentId, AgentOperation.PayPassword, editUsername)));
    }
}
