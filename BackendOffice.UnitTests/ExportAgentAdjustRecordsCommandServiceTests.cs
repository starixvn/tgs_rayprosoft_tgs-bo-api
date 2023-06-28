using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.BrandCertificate.Clients;
using Tgs.CashQueryService.Client;
using Tgs.PlayerService.Client;
using TGS.Services.BackOffice.ApplicationServices.Adjust;
using TGS.Services.BackOffice.ApplicationServices;
using Tgs.ServiceUtility;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;

namespace TGS.Tests.BackendOffice.UnitTests;
public class ExportAgentAdjustRecordsCommandServiceTests
{
    [Test]
    public async Task 匯出代理調帳記錄()
    {
        var fakeCashQueryClient = Substitute.For<ICashQueryClient>();
        var fakeBrandRelationQueryService = Substitute.For<IQueryService<BrandRelationQuery, BrandRelationInfo>>();
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new ExportAdjustRecordsCommandService(
            fakeCashQueryClient,
            fakeBrandRelationQueryService,
            fakeAgentClient,
            fakePlayerServiceClient);

        using var fs = new StubWriteStream();

        var command = new ExportAgentAdjustRecordsCommand(
            Guid.NewGuid(),
            (DateTime.Now.AddDays(-1D), DateTime.Now),
            fs);

        var agentId = Guid.NewGuid();

        _ = fakeCashQueryClient.CountAgentAdjustRecordsAsync(
            Arg.Is(command.BrandId),
            Arg.Any<AdjustRecordQueryCondition>())
            .Returns(1);
        _ = fakeCashQueryClient.QueryAgentAdjustRecordsAsync(
            Arg.Is(command.BrandId),
            Arg.Any<AdjustRecordQueryCondition>(),
            Arg.Any<int>())
            .Returns(new[]
            {
                new AdjustRecordListItem(
                    Guid.NewGuid(),
                    command.BrandId,
                    agentId.ToString(),
                    Tgs.CashQueryService.AdjustType.AgentOther,
                    100M,
                    500M,
                    "Manager",
                    "123",
                    DateTime.Now,
                    Tgs.CashQueryService.UserType.Agent)
            }.ToAsyncEnumerable());

        _ = fakeBrandRelationQueryService.QueryAsync(Arg.Any<BrandRelationQuery>())
            .Returns(new[]
            {
                new BrandRelationInfo(
                    command.BrandId,
                    "AAA",
                    "AAA",
                    Guid.NewGuid(),
                    string.Empty,
                    string.Empty,
                    Guid.NewGuid(),
                    string.Empty,
                    string.Empty)
            }.ToAsyncEnumerable());

        _ = fakeAgentClient.GetAgentInfoAsync(Arg.Is(new GetAgentInfoRequest(agentId)))
            .Returns(new AgentInfoModel(
                agentId,
                "agent1",
                null,
                null,
                null,
                null,
                null,
                string.Empty,
                null,
                DateTimeOffset.Now,
                Tgs.Agent.Client.Models.Enums.AgentStatus.Enabled,
                Tgs.Agent.Client.Models.Enums.AgentType.Cash,
                Tgs.Agent.Client.Models.Enums.AgentLevelType.Agent,
                0M,
                0,
                0M));

        await sut.ExecuteAsync(command);

        Assert.Greater(fs.Length, 0L);
    }
}
