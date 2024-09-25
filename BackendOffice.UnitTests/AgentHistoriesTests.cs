using System.Text.Json;
using NUnit.Framework.Legacy;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.Agent.Client.Models.Enums;
using TGS.Services.BackOffice.ApplicationServices.Agents;

namespace TGS.Tests.BackendOffice.UnitTests;

internal class AgentHistoriesTests
{
    [Test]
    public async Task 查詢代理歷史紀錄()
    {
        var fakeAgentClient = Substitute.For<IAgentClient>();

        var sut = new AgentHistoriesQueryService(fakeAgentClient);

        var agentId = Guid.NewGuid();
        var from = DateTime.Now.AddDays(-14);
        var to = DateTime.Now;

        _ = fakeAgentClient
            .CountAgentHistoriesAsync(new GetAgentHistoriesQueryConditions(agentId, from, to))
            .Returns(20);

        _ = await sut.QueryAsync(new AgentHistoriesQuery(agentId, from, to)).ToArrayAsync();

        var conditions = new GetAgentHistoriesQueryConditions(agentId, from, to);

        _ = fakeAgentClient
            .Received(1)
            .GetAgentHistoriesAsync(Arg.Is(conditions), Arg.Is(20));
    }

    [Test]
    public async Task 查詢代理歷史紀錄筆數()
    {
        var fakeAgentClient = Substitute.For<IAgentClient>();

        var sut = new AgentHistoriesQueryService(fakeAgentClient);

        var agentId = Guid.NewGuid();
        var from = DateTime.Now.AddDays(-14);
        var to = DateTime.Now;

        _ = fakeAgentClient
            .CountAgentHistoriesAsync(new GetAgentHistoriesQueryConditions(agentId, from, to))
            .Returns(20);

        _ = await sut.GetAsync(new AgentHistoriesQuery(agentId, from, to));

        var conditions = new GetAgentHistoriesQueryConditions(agentId, from, to);

        _ = fakeAgentClient
            .Received(1)
            .CountAgentHistoriesAsync(Arg.Is(conditions));
    }

    [Test]
    public async Task 查詢代理歷史紀錄內容()
    {
        var fakeAgentClient = Substitute.For<IAgentClient>();

        var sut = new AgentHistoryQueryService(fakeAgentClient);

        var eventId = Guid.NewGuid();

        _ = fakeAgentClient
            .GetAgentHistoryAsync(Arg.Is(eventId))
            .Returns(new AgentEventInfo()
            {
                Action = EventAction.Create,
                ChangeSets = JsonDocument.Parse("{}"),
                CreatedOn = DateTime.Now.AddDays(-1)
            });

        var actual = await sut.GetAsync(new AgentHistoryQuery(eventId));

        ClassicAssert.AreEqual(EventAction.Create, actual.Action);
    }
}
