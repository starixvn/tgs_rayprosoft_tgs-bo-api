using NSubstitute;
using NUnit.Framework.Legacy;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.BrandCertificate.Clients;
using Tgs.CashQueryService.Client;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.ApplicationServices.Adjust;

namespace TGS.Tests.BackendOffice.UnitTests;

public class ExportAdjustRecordsCommandServiceTests
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
                    Tgs.CashQueryService.OperationMode.Add,
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
                0M,
                Guid.NewGuid(),
                Guid.NewGuid(),
                null,
                false,
                true));

        await sut.ExecuteAsync(command);

        ClassicAssert.Greater(fs.Length, 0L);
    }

    [Test]
    public async Task 匯出全部調帳記錄()
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

        var brandId = Guid.NewGuid();
        var command = new ExportAllAdjustRecordsCommand(
            [brandId],
            (DateTime.Now.AddDays(-1D), DateTime.Now),
            fs);

        _ = fakeCashQueryClient.CountPlayerAdjustRecordsAsync(Arg.Is(brandId), Arg.Any<AdjustRecordQueryCondition>())
            .Returns(1);
        _ = fakeCashQueryClient.QueryPlayerAdjustRecordsAsync(
            Arg.Is(brandId),
            Arg.Any<AdjustRecordQueryCondition>(),
            Arg.Any<int>())
            .Returns(new[]
            {
                new AdjustRecordListItem(
                    Guid.NewGuid(),
                    brandId,
                    "test1",
                    Tgs.CashQueryService.AdjustType.CompanyAdjust,
                    Tgs.CashQueryService.OperationMode.Add,
                    100M,
                    500M,
                    "Manager",
                    "123",
                    DateTime.Now,
                    Tgs.CashQueryService.UserType.Player)
            }.ToAsyncEnumerable());

        _ = fakeBrandRelationQueryService.QueryAsync(Arg.Any<BrandRelationQuery>())
            .Returns(new[]
            {
                new BrandRelationInfo(
                    brandId,
                    "AAA",
                    "AAA",
                    Guid.NewGuid(),
                    string.Empty,
                    string.Empty,
                    Guid.NewGuid(),
                    string.Empty,
                    string.Empty)
            }.ToAsyncEnumerable());

        _ = fakePlayerServiceClient.QueryPlayerInfoListAsync(Arg.Is(brandId), Arg.Any<IEnumerable<string>>())
            .Returns(new[]
            {
                new PlayerInfo
                {
                    PlayerCode = "test1",
                    UserName = "Player1"
                }
            }.ToAsyncEnumerable());

        var agentId = Guid.NewGuid();

        _ = fakeCashQueryClient.CountAgentAdjustRecordsAsync(
            Arg.Is(brandId),
            Arg.Any<AdjustRecordQueryCondition>())
            .Returns(1);
        _ = fakeCashQueryClient.QueryAgentAdjustRecordsAsync(
            Arg.Is(brandId),
            Arg.Any<AdjustRecordQueryCondition>(),
            Arg.Any<int>())
            .Returns(new[]
            {
                new AdjustRecordListItem(
                    Guid.NewGuid(),
                    brandId,
                    agentId.ToString(),
                    Tgs.CashQueryService.AdjustType.AgentOther,
                    Tgs.CashQueryService.OperationMode.Add,
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
                    brandId,
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
                0M,
                brandId,
                Guid.NewGuid(),
                null,
                false,
                true));

        await sut.ExecuteAsync(command);

        ClassicAssert.Greater(fs.Length, 0L);
    }

    [Test]
    public async Task 匯出玩家調帳記錄()
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

        var command = new ExportPlayerAdjustRecordsCommand(
            Guid.NewGuid(),
            (DateTime.Now.AddDays(-1D), DateTime.Now),
            fs);

        _ = fakeCashQueryClient.CountPlayerAdjustRecordsAsync(Arg.Is(command.BrandId), Arg.Any<AdjustRecordQueryCondition>())
            .Returns(1);
        _ = fakeCashQueryClient.QueryPlayerAdjustRecordsAsync(
            Arg.Is(command.BrandId),
            Arg.Any<AdjustRecordQueryCondition>(),
            Arg.Any<int>())
            .Returns(new[]
            {
                new AdjustRecordListItem(
                    Guid.NewGuid(),
                    command.BrandId,
                    "test1",
                    Tgs.CashQueryService.AdjustType.CompanyAdjust,
                    Tgs.CashQueryService.OperationMode.Add,
                    100M,
                    500M,
                    "Manager",
                    "123",
                    DateTime.Now,
                    Tgs.CashQueryService.UserType.Player)
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

        _ = fakePlayerServiceClient.QueryPlayerInfoListAsync(Arg.Is(command.BrandId), Arg.Any<IEnumerable<string>>())
            .Returns(new[]
            {
                new PlayerInfo
                {
                    PlayerCode = "test1",
                    UserName = "Player1"
                }
            }.ToAsyncEnumerable());

        await sut.ExecuteAsync(command);

        ClassicAssert.Greater(fs.Length, 0L);
    }
}
