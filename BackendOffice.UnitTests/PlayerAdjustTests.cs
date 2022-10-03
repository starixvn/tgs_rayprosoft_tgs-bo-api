using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Services;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests;

public class PlayerAdjustTests
{
    [Test]
    public async Task Adjust_Player_Balance()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePlayerAdjustBalanceService = Substitute.For<IPlayerAdjustBalanceService>();

        var sut = new AdjustPlayerBalanceCommandService(
            fakePlayerServiceClient,
            fakePlayerAdjustBalanceService);

        var command = new AdjustPlayerBalanceCommand(
            "test1",
            Core.Models.Enums.AdjustmentType.Add,
            Core.Models.Enums.PlayerAdjustmentTransactionType.BonusAdjust,
            100M,
            100M,
            "remark",
            true,
            Guid.NewGuid(),
            "modifier");

        var applyInfo = new PlayerBalanceAdjustmentApplyInfo(
            Guid.NewGuid(),
            command.PlayerCode,
            Guid.NewGuid(),
            command.AdjustmentType,
            command.PlayerAdjustmentTransactionType,
            AdjustTransferType.None,
            command.Amount,
            command.BucketAmount,
            command.IncludeInNetProfit,
            command.Remark);

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(command.PlayerCode))
            .Returns(new Tgs.PlayerService.Client.PlayerInfo
            {
                PlayerCode = command.PlayerCode,
                BrandId = applyInfo.BrandId
            });

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(command.PlayerCode))
            .Returns(new Dictionary<string, string>
            {
                ["DefaultWalletId"] = applyInfo.WalletId.ToString(),
            }.ToImmutableDictionary());

        await sut.ExecuteAsync(command);

        _ = fakePlayerAdjustBalanceService.Received(1)
            .ProcessAdjustApplyAsync(
                Arg.Is(applyInfo),
                Arg.Is(command.ModifyUserId),
                Arg.Is(command.ModifyUser));
    }

    [Test]
    public async Task Transfer_Player_Balance()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeQueryAgentsService = Substitute.For<IQueryService<AllAgentsByBrandQuery, AgentInfo>>();
        var fakePlayerAdjustBalanceService = Substitute.For<IPlayerAdjustBalanceService>();

        var sut = new PlayerTransferBalanceToParentAgentCommandService(
            fakePlayerServiceClient,
            fakeQueryAgentsService,
            fakePlayerAdjustBalanceService);

        var command = new PlayerTransferBalanceToParentAgentCommand(
            "test1",
            100M,
            100M,
            "remark",
            true,
            Guid.NewGuid());

        var applyInfo = new PlayerBalanceTransferApplyInfo(
            Guid.NewGuid(),
            command.PlayerCode,
            Guid.NewGuid(),
            command.Amount,
            command.BucketAmount,
            command.IncludeInNetProfit,
            command.Remark,
            Guid.NewGuid(),
            Guid.NewGuid());

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(command.PlayerCode))
            .Returns(new Tgs.PlayerService.Client.PlayerInfo
            {
                PlayerCode = command.PlayerCode,
                BrandId = applyInfo.BrandId
            });

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(command.PlayerCode))
            .Returns(new Dictionary<string, string>
            {
                ["DefaultWalletId"] = applyInfo.WalletId.ToString(),
                ["AgentId"] = applyInfo.TargetAgent.ToString()
            }.ToImmutableDictionary());

        _ = fakeQueryAgentsService.QueryAsync(Arg.Any<AllAgentsByBrandQuery>())
            .Returns(new[] {
                new AgentInfo(
                    applyInfo.TargetAgent,
                    "agent1",
                    applyInfo.TargetAgentWalletId,
                    AgentStatus.Enabled)
            }.ToAsyncEnumerable());

        await sut.ExecuteAsync(command);

        _ = fakePlayerAdjustBalanceService.Received(1)
            .ProcessTransferApplyAsync(
                Arg.Is(applyInfo),
                Arg.Is(command.ModifyUserId));
    }
}
