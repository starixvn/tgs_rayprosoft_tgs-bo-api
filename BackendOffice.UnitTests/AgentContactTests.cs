using System.Collections.Immutable;
using NSubstitute;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.Agent.Client.Models.Enums;
using TGS.Services.BackOffice.ApplicationServices.Agents;

namespace TGS.Tests.BackendOffice.UnitTests;

public class AgentContactTests
{
    [Test]
    public async Task 變更代理聯繫信息()
    {
        var fakeAgentClient = Substitute.For<IAgentClient>();

        var sut = new SetAgentContactCommandService(fakeAgentClient);

        var agentId = Guid.NewGuid();
        var modifyUserId = "6a379883-d8d2-4ffb-3d05-08d9262ce1a5";
        var modifyUsername = "username";
        var phone = "newPhone";

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
                0m));

        var queryDict = new Dictionary<string, string?>
        {
            { nameof(AgentPropertyName.Phone), phone }
        };

        var actual = await sut.GetAsync(new SetAgentContactCommand(agentId, queryDict.ToImmutableDictionary(), modifyUserId, modifyUsername));

        var modifyDict = new Dictionary<AgentPropertyName, string?>
        {
            { AgentPropertyName.Phone, phone }
        };

        _ = fakeAgentClient
            .Received(1)
            .SetAgentPropertiesAsync(
                Arg.Is(agentId),
                Arg.Is<ImmutableDictionary<AgentPropertyName, string?>>(x => x.SequenceEqual(modifyDict.ToImmutableDictionary())),
                Arg.Is(modifyUserId));

        var auditRequest = new AddAgentAuditLogRequest(agentId, AgentOperation.Contact, modifyUsername);

        _ = fakeAgentClient
            .Received(1)
            .AddAgentAuditLogAsync(Arg.Is(auditRequest));

        Assert.True(actual.Valid);
    }
}
