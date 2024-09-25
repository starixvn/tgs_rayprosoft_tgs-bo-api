using System.Collections.Immutable;
using NUnit.Framework.Legacy;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.Agent.Client.Models.Enums;
using TGS.Services.BackOffice.ApplicationServices.Agents;

namespace TGS.Tests.BackendOffice.UnitTests;

public class AgentInfoTests
{
    [Test]
    public async Task 變更代理資訊()
    {
        var fakeAgentClient = Substitute.For<IAgentClient>();

        var sut = new SetAgentInfoCommandService(fakeAgentClient);

        var agentId = Guid.NewGuid();
        var remark = "test remark";
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

        var queryDict = new Dictionary<string, string?>()
        {
            { nameof(AgentPropertyName.Remarks), remark }
        };

        var actual = await sut.GetAsync(new SetAgentInfoCommand(agentId, queryDict, modifyUserId, modifyUsername));

        var modifyDict = new Dictionary<AgentPropertyName, string?>
        {
            { AgentPropertyName.Remarks, remark }
        };

        _ = fakeAgentClient
            .Received(1)
            .SetAgentPropertiesAsync(
                Arg.Is(agentId),
                Arg.Is<ImmutableDictionary<AgentPropertyName, string?>>(x => x.SequenceEqual(modifyDict)),
                Arg.Is(modifyUserId));

        _ = fakeAgentClient
            .Received(1)
            .AddAgentAuditLogAsync(Arg.Is(new AddAgentAuditLogRequest(agentId, AgentOperation.Info, modifyUsername)));

        ClassicAssert.True(actual.Valid);
    }
}
