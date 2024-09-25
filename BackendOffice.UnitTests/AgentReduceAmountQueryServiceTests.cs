using System.Collections.Immutable;
using NUnit.Framework.Legacy;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.CashService.Client;
using Tgs.PlayerService.Client;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using TGS.Services.BackOffice.Infrastructure.DataService;
using Resources = TGS.Services.BackOffice.Application.Resources;

namespace TGS.Tests.BackendOffice.UnitTests;

public class AgentReduceAmountQueryServiceTests
{
    [Test]
    public async Task 代理調帳_扣錢()
    {
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerService = Substitute.For<IPlayerServiceClient>();
        var fakeAdjustAgentAccountService = Substitute.For<IAdjustAgentAccountService>();
        var fakeAdjustTransferService = Substitute.For<IAdjustTransferService>();

        var sut = new AgentReduceAmountCommandService(
            fakeAgentClient,
            fakeCashServiceClient,
            fakePlayerService,
            fakeAdjustAgentAccountService,
            fakeAdjustTransferService);

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        var command = new AgentReduceAmountCommand(
            Guid.Parse("5056ebbf-a975-42f1-9e97-b3299f0cab06"),
            168M,
            "testRemark",
            null,
            AgentAdjustmentTransactionType.Bonus,
            AccountType.Agent,
            "MONEY",
            Guid.NewGuid());

        var agentWalletId = Guid.NewGuid();

        _ = fakeAgentClient.GetAgentInfoAsync(
            Arg.Is(new GetAgentInfoRequest(command.AgentId)))
            .Returns(new AgentInfoModel(
                command.AgentId,
                "test1",
                null,
                null,
                null,
                null,
                null,
                "abcd",
                null,
                DateTimeOffset.UtcNow,
                Tgs.Agent.Client.Models.Enums.AgentStatus.Enabled,
                Tgs.Agent.Client.Models.Enums.AgentType.Cash,
                Tgs.Agent.Client.Models.Enums.AgentLevelType.Agent,
                0M,
                0,
                0M,
                brandId,
                Guid.NewGuid(),
                agentWalletId, 
                false,
                true));

        var transactionId = Guid.NewGuid();
        _ = fakeCashServiceClient.AdjustCompanyV2Async(
            Arg.Is(agentWalletId),
            Arg.Is(-command.Amount),
            Arg.Is(command.Remark),
            Arg.Any<string>(),
            Arg.Is(command.UserId))
            .Returns(new TransactionResponse
            {
                TransactionId = transactionId
            });

        var actual = await sut.GetAsync(command);

        ClassicAssert.True(actual.Valid);
        _ = fakeAdjustAgentAccountService.Received(1)
            .CreateAsync(
                Arg.Is(transactionId),
                Arg.Is(brandId),
                Arg.Is(command.AgentId),
                Arg.Is(AdjustmentType.Subtract),
                Arg.Is(command.TransactionType),
                Arg.Is(AdjustTransferType.None),
                Arg.Is(command.Amount),
                Arg.Any<string>(),
                Arg.Is(command.Username));
    }

    [Test]
    public async Task 代理轉帳代理_目標代理非直屬代理或上層代理()
    {
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerService = Substitute.For<IPlayerServiceClient>();
        var fakeAdjustAgentAccountService = Substitute.For<IAdjustAgentAccountService>();
        var fakeAdjustTransferService = Substitute.For<IAdjustTransferService>();

        var sut = new AgentReduceAmountCommandService(
            fakeAgentClient,
            fakeCashServiceClient,
            fakePlayerService,
            fakeAdjustAgentAccountService,
            fakeAdjustTransferService);

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        var command = new AgentReduceAmountCommand(
            Guid.Parse("5056ebbf-a975-42f1-9e97-b3299f0cab06"),
            168M,
            "testRemark",
            "1shaun",
            AgentAdjustmentTransactionType.Transfer,
            AccountType.Agent,
            "MONEY",
            Guid.NewGuid());

        _ = fakeAgentClient.GetAgentInfoAsync(
            Arg.Is(new GetAgentInfoRequest(command.AgentId)))
            .Returns(new AgentInfoModel(
                command.AgentId,
                "test1",
                null,
                null,
                null,
                null,
                null,
                "abcd",
                null,
                DateTimeOffset.UtcNow,
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

        _ = fakeAgentClient.GetAgentInfoByUsernameAsync(
            Arg.Is(new GetAgentInfoByUsernameRequest(brandId, command.TargetName)))
            .Returns(new AgentInfoModel(
                command.AgentId,
                "test1",
                null,
                null,
                null,
                null,
                null,
                "abcd",
                null,
                DateTimeOffset.UtcNow,
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

        var actual = await sut.GetAsync(command);

        ClassicAssert.False(actual.Valid);
        ClassicAssert.AreEqual(Resources.Agent.TargetAgentNotHomologue, actual.Message);
    }

    [Test]
    public async Task 代理轉帳代理_找不到目標代理()
    {
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerService = Substitute.For<IPlayerServiceClient>();
        var fakeAdjustAgentAccountService = Substitute.For<IAdjustAgentAccountService>();
        var fakeAdjustTransferService = Substitute.For<IAdjustTransferService>();

        var sut = new AgentReduceAmountCommandService(
            fakeAgentClient,
            fakeCashServiceClient,
            fakePlayerService,
            fakeAdjustAgentAccountService,
            fakeAdjustTransferService);

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        var command = new AgentReduceAmountCommand(
            Guid.Parse("5056ebbf-a975-42f1-9e97-b3299f0cab06"),
            168M,
            "testRemark",
            "1shaun",
            AgentAdjustmentTransactionType.Transfer,
            AccountType.Agent,
            "MONEY",
            Guid.NewGuid());

        _ = fakeAgentClient.GetAgentInfoAsync(
            Arg.Is(new GetAgentInfoRequest(command.AgentId)))
            .Returns(new AgentInfoModel(
                command.AgentId,
                "test1",
                null,
                null,
                null,
                null,
                null,
                "abcd",
                null,
                DateTimeOffset.UtcNow,
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

        var actual = await sut.GetAsync(command);

        ClassicAssert.False(actual.Valid);
        ClassicAssert.AreEqual(Resources.Agent.TargetAgentNotFound, actual.Message);
    }

    [Test]
    public async Task 代理轉帳代理成功()
    {
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerService = Substitute.For<IPlayerServiceClient>();
        var fakeAdjustAgentAccountService = Substitute.For<IAdjustAgentAccountService>();
        var fakeAdjustTransferService = Substitute.For<IAdjustTransferService>();

        var sut = new AgentReduceAmountCommandService(
            fakeAgentClient,
            fakeCashServiceClient,
            fakePlayerService,
            fakeAdjustAgentAccountService,
            fakeAdjustTransferService);

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        var command = new AgentReduceAmountCommand(
            Guid.Parse("5056ebbf-a975-42f1-9e97-b3299f0cab06"),
            168M,
            "testRemark",
            "1shaun",
            AgentAdjustmentTransactionType.Transfer,
            AccountType.Agent,
            "MONEY",
            Guid.NewGuid());

        var agentWalletId = Guid.NewGuid();
        var targetWalletId = Guid.NewGuid();

        _ = fakeAgentClient.GetAgentInfoAsync(
            Arg.Is(new GetAgentInfoRequest(command.AgentId)))
            .Returns(new AgentInfoModel(
                command.AgentId,
                "test1",
                null,
                null,
                null,
                null,
                null,
                "abcd",
                null,
                DateTimeOffset.UtcNow,
                Tgs.Agent.Client.Models.Enums.AgentStatus.Enabled,
                Tgs.Agent.Client.Models.Enums.AgentType.Cash,
                Tgs.Agent.Client.Models.Enums.AgentLevelType.Agent,
                0M,
                0,
                0M,
                brandId,
                Guid.NewGuid(),
                agentWalletId, 
                false,
                true));

        _ = fakeAgentClient.GetAgentInfoByUsernameAsync(
            Arg.Is(new GetAgentInfoByUsernameRequest(brandId, command.TargetName)))
            .Returns(new AgentInfoModel(
                Guid.NewGuid(),
                command.TargetName,
                null,
                null,
                null,
                null,
                null,
                "abcd",
                command.AgentId,
                DateTimeOffset.UtcNow,
                Tgs.Agent.Client.Models.Enums.AgentStatus.Enabled,
                Tgs.Agent.Client.Models.Enums.AgentType.Cash,
                Tgs.Agent.Client.Models.Enums.AgentLevelType.Agent,
                0M,
                0,
                0M,
                brandId,
                Guid.NewGuid(),
                targetWalletId,
                false,
                true));

        var lockTransactionId = Guid.NewGuid();
        _ = fakeCashServiceClient.LockTransferAsync(
            Arg.Is(agentWalletId),
            Arg.Is(command.Amount),
            Arg.Is(command.Remark),
            Arg.Any<Guid>(),
            Arg.Is(command.UserId))
            .Returns(new TransactionResponse
            {
                TransactionId = lockTransactionId
            });

        var adjustFormId = Guid.NewGuid();
        _ = fakeAdjustAgentAccountService.CreateAsync(
            Arg.Is(lockTransactionId),
            Arg.Is(brandId),
            Arg.Is(command.AgentId),
            Arg.Is(AdjustmentType.Subtract),
            Arg.Is(command.TransactionType),
            Arg.Is(AdjustTransferType.None),
            Arg.Is(command.Amount),
            Arg.Any<string>(),
            Arg.Is(command.Username))
            .Returns(new AdjustAgentRecord
            {
                Id = adjustFormId,
                BrandId = brandId
            });

        var lockReceiveId = Guid.NewGuid();
        _ = fakeCashServiceClient.LockReceiveAsync(
            Arg.Is(targetWalletId),
            Arg.Is(command.Amount),
            Arg.Is(command.Remark),
            Arg.Is(adjustFormId),
            Arg.Is(command.UserId))
            .Returns(new TransactionResponse
            {
                TransactionId = lockReceiveId
            });

        var actual = await sut.GetAsync(command);

        ClassicAssert.True(actual.Valid);

        _ = fakeAdjustTransferService.Received(1)
            .CreateAsync(
                Arg.Is(lockTransactionId),
                Arg.Is(lockReceiveId),
                Arg.Is(AdjustTransferType.AgentToAgent),
                Arg.Is("test1"),
                Arg.Is(command.TargetName),
                Arg.Is(command.Amount));
        _ = fakeCashServiceClient.Received(1)
            .CommitTransferAsync(
                Arg.Is(agentWalletId),
                Arg.Is(command.Amount),
                Arg.Is(command.Remark),
                Arg.Is(adjustFormId),
                Arg.Is(command.UserId));
        _ = fakeCashServiceClient.Received(1)
            .CommitReceiveAsync(
                Arg.Is(targetWalletId),
                Arg.Is(command.Amount),
                Arg.Is(command.Remark),
                Arg.Is(adjustFormId),
                Arg.Is(command.UserId));
    }

    [Test]
    public async Task 代理轉帳玩家_找不到目標玩家()
    {
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerService = Substitute.For<IPlayerServiceClient>();
        var fakeAdjustAgentAccountService = Substitute.For<IAdjustAgentAccountService>();
        var fakeAdjustTransferService = Substitute.For<IAdjustTransferService>();

        var sut = new AgentReduceAmountCommandService(
            fakeAgentClient,
            fakeCashServiceClient,
            fakePlayerService,
            fakeAdjustAgentAccountService,
            fakeAdjustTransferService);

        var brandId = Guid.NewGuid();

        var command = new AgentReduceAmountCommand(
            Guid.Parse("5056ebbf-a975-42f1-9e97-b3299f0cab06"),
            168M,
            "testRemark",
            "shaun01",
            AgentAdjustmentTransactionType.Transfer,
            AccountType.Player,
            "MONEY",
            Guid.NewGuid());

        _ = fakeAgentClient.GetAgentInfoAsync(
            Arg.Is(new GetAgentInfoRequest(command.AgentId)))
            .Returns(new AgentInfoModel(
                command.AgentId,
                "test1",
                null,
                null,
                null,
                null,
                null,
                "abcd",
                null,
                DateTimeOffset.UtcNow,
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

        var actual = await sut.GetAsync(command);

        ClassicAssert.False(actual.Valid);
        ClassicAssert.AreEqual(Resources.Agent.TargetPlayerNotFound, actual.Message);
    }

    [Test]
    public async Task 代理轉帳玩家成功()
    {
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerService = Substitute.For<IPlayerServiceClient>();
        var fakeAdjustAgentAccountService = Substitute.For<IAdjustAgentAccountService>();
        var fakeAdjustTransferService = Substitute.For<IAdjustTransferService>();

        var sut = new AgentReduceAmountCommandService(
            fakeAgentClient,
            fakeCashServiceClient,
            fakePlayerService,
            fakeAdjustAgentAccountService,
            fakeAdjustTransferService);

        var brandId = Guid.NewGuid();

        var command = new AgentReduceAmountCommand(
            Guid.Parse("5056ebbf-a975-42f1-9e97-b3299f0cab06"),
            168M,
            "testRemark",
            "shaun01",
            AgentAdjustmentTransactionType.Transfer,
            AccountType.Player,
            "MONEY",
            Guid.NewGuid());

        var agentWalletId = Guid.NewGuid();
        _ = fakeAgentClient.GetAgentInfoAsync(
            Arg.Is(new GetAgentInfoRequest(command.AgentId)))
            .Returns(new AgentInfoModel(
                command.AgentId,
                "test1",
                null,
                null,
                null,
                null,
                null,
                "abcd",
                null,
                DateTimeOffset.UtcNow,
                Tgs.Agent.Client.Models.Enums.AgentStatus.Enabled,
                Tgs.Agent.Client.Models.Enums.AgentType.Cash,
                Tgs.Agent.Client.Models.Enums.AgentLevelType.Agent,
                0M,
                0,
                0M,
                brandId,
                Guid.NewGuid(),
                agentWalletId, 
                false,
                true));

        var playerDefaultWalletId = Guid.Parse("af5effa8-6352-4483-36cc-08dab0b34fcf");
        var player = new Tgs.PlayerService.Client.PlayerInfo
        {
            PlayerCode = "test1",
            UserName = command.TargetName
        };

        _ = fakePlayerService.GetPlayerInfoByUserNameAsync(
            Arg.Is(brandId),
            Arg.Is(command.TargetName))
            .Returns(player);
        _ = fakePlayerService.GetPlayerPropertiesAsync(Arg.Is(player.PlayerCode))
            .Returns(new Dictionary<string, string>
            {
                ["AgentId"] = command.AgentId.ToString(),
                ["DefaultWalletId"] = playerDefaultWalletId.ToString()
            }.ToImmutableDictionary());

        var lockTransactionId = Guid.NewGuid();
        _ = fakeCashServiceClient.LockTransferAsync(
            Arg.Is(agentWalletId),
            Arg.Is(command.Amount),
            Arg.Is(command.Remark),
            Arg.Any<Guid>(),
            Arg.Is(command.UserId))
            .Returns(new TransactionResponse
            {
                TransactionId = lockTransactionId
            });

        var adjustFormId = Guid.NewGuid();
        _ = fakeAdjustAgentAccountService.CreateAsync(
            Arg.Is(lockTransactionId),
            Arg.Is(brandId),
            Arg.Is(command.AgentId),
            Arg.Is(AdjustmentType.Subtract),
            Arg.Is(command.TransactionType),
            Arg.Is(AdjustTransferType.None),
            Arg.Is(command.Amount),
            Arg.Any<string>(),
            Arg.Is(command.Username))
            .Returns(new AdjustAgentRecord
            {
                Id = adjustFormId,
                BrandId = brandId
            });

        var lockReceiveId = Guid.NewGuid();
        _ = fakeCashServiceClient.LockReceiveAsync(
            Arg.Is(playerDefaultWalletId),
            Arg.Is(command.Amount),
            Arg.Is(command.Remark),
            Arg.Is(adjustFormId),
            Arg.Is(command.UserId))
            .Returns(new TransactionResponse
            {
                TransactionId = lockReceiveId
            });

        var actual = await sut.GetAsync(command);

        ClassicAssert.True(actual.Valid);
        _ = fakeAdjustTransferService.Received(1)
            .CreateAsync(
                Arg.Is(lockTransactionId),
                Arg.Is(lockReceiveId),
                Arg.Is(AdjustTransferType.AgentToPlayer),
                Arg.Is("test1"),
                Arg.Is(command.TargetName),
                Arg.Is(command.Amount));
        _ = fakeCashServiceClient.Received(1)
            .CommitTransferAsync(
                Arg.Is(agentWalletId),
                Arg.Is(command.Amount),
                Arg.Is(command.Remark),
                Arg.Is(adjustFormId),
                Arg.Is(command.UserId));
        _ = fakeCashServiceClient.Received(1)
            .CommitReceiveAsync(
                Arg.Is(playerDefaultWalletId),
                Arg.Is(command.Amount),
                Arg.Is(command.Remark),
                Arg.Is(adjustFormId),
                Arg.Is(command.UserId));
    }

    [Test]
    public async Task 代理轉帳給非直屬玩家()
    {
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerService = Substitute.For<IPlayerServiceClient>();
        var fakeAdjustAgentAccountService = Substitute.For<IAdjustAgentAccountService>();
        var fakeAdjustTransferService = Substitute.For<IAdjustTransferService>();

        var sut = new AgentReduceAmountCommandService(
            fakeAgentClient,
            fakeCashServiceClient,
            fakePlayerService,
            fakeAdjustAgentAccountService,
            fakeAdjustTransferService);

        var brandId = Guid.NewGuid();

        var command = new AgentReduceAmountCommand(
            Guid.Parse("5056ebbf-a975-42f1-9e97-b3299f0cab06"),
            168M,
            "testRemark",
            "shaun01",
            AgentAdjustmentTransactionType.Transfer,
            AccountType.Player,
            "MONEY",
            Guid.NewGuid());

        var agentWalletId = Guid.NewGuid();
        _ = fakeAgentClient.GetAgentInfoAsync(
            Arg.Is(new GetAgentInfoRequest(command.AgentId)))
            .Returns(new AgentInfoModel(
                command.AgentId,
                "test1",
                null,
                null,
                null,
                null,
                null,
                "abcd",
                null,
                DateTimeOffset.UtcNow,
                Tgs.Agent.Client.Models.Enums.AgentStatus.Enabled,
                Tgs.Agent.Client.Models.Enums.AgentType.Cash,
                Tgs.Agent.Client.Models.Enums.AgentLevelType.Agent,
                0M,
                0,
                0M,
                brandId,
                Guid.NewGuid(),
                agentWalletId,
                false,
                true));

        var playerDefaultWalletId = Guid.Parse("af5effa8-6352-4483-36cc-08dab0b34fcf");
        var player = new Tgs.PlayerService.Client.PlayerInfo
        {
            PlayerCode = "test1",
            UserName = command.TargetName
        };

        _ = fakePlayerService.GetPlayerInfoByUserNameAsync(
            Arg.Is(brandId),
            Arg.Is(command.TargetName))
            .Returns(player);
        _ = fakePlayerService.GetPlayerPropertiesAsync(Arg.Is(player.PlayerCode))
            .Returns(new Dictionary<string, string>
            {
                ["AgentId"] = Guid.NewGuid().ToString(),
                ["DefaultWalletId"] = playerDefaultWalletId.ToString()
            }.ToImmutableDictionary());

        var actual = await sut.GetAsync(command);

        ClassicAssert.False(actual.Valid);
        ClassicAssert.AreEqual(Resources.Agent.TargetPlayerNotUnderAgent, actual.Message);
    }

    [Test]
    public async Task 找不到代理()
    {
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerService = Substitute.For<IPlayerServiceClient>();
        var fakeAdjustAgentAccountService = Substitute.For<IAdjustAgentAccountService>();
        var fakeAdjustTransferService = Substitute.For<IAdjustTransferService>();

        var sut = new AgentReduceAmountCommandService(
            fakeAgentClient,
            fakeCashServiceClient,
            fakePlayerService,
            fakeAdjustAgentAccountService,
            fakeAdjustTransferService);

        var command = new AgentReduceAmountCommand(
            Guid.Parse("5056ebbf-a975-42f1-9e97-b3299f0cab06"),
            168M,
            "testRemark",
            "shaun01",
            AgentAdjustmentTransactionType.Transfer,
            AccountType.Player,
            "MONEY",
            Guid.NewGuid());

        var actual = await sut.GetAsync(command);

        ClassicAssert.False(actual.Valid);
        ClassicAssert.AreEqual(Resources.Agent.NotFound, actual.Message);
    }
}
