using System.Linq.Expressions;
using NSubstitute;
using Tgs.CashService.Client;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Json;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using TGS.Services.BackOffice.Infrastructure.DataService;
using TGS.Services.WalletService;
using Resources = TGS.Services.BackOffice.Application.Resources;

namespace TGS.Tests.BackendOffice.UnitTests;

public class AgentReduceAmountQueryServiceTests
{
    [Test]
    public async Task 代理轉帳代理_目標代理非直屬代理或上層代理()
    {
        var fakeAgentService = Substitute.For<IAgentService>();
        var fakeAgentWalletRepository = Substitute.For<IAgentWalletRepository>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerService = Substitute.For<IPlayerService>();
        var fakeJsonSerializationProvider = Substitute.For<IJsonSerializationProvider>();
        var fakeAdjustAgentAccountService = Substitute.For<IAdjustAgentAccountService>();
        var fakeAdjustTransferService = Substitute.For<IAdjustTransferService>();

        var sut = new AgentReduceAmountQueryService(
            fakeAgentService,
            fakeAgentWalletRepository,
            fakeCashServiceClient,
            fakePlayerService,
            fakeJsonSerializationProvider,
            fakeAdjustAgentAccountService, fakeAdjustTransferService);

        var agentId = Guid.Parse("5056ebbf-a975-42f1-9e97-b3299f0cab06");
        var amount = 168M;
        var userId = "2d26bf36-3f32-4874-994b-08da91f9a401";
        var playerDefaultWalletId = Guid.Parse("af5effa8-6352-4483-36cc-08dab0b34fcf");
        var remark = "testRemark";
        var targetAgentName = "1shaun";
        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        _ = fakeAgentService
            .GetAgentAsync(Arg.Any<Expression<Func<Agent, bool>>>())
            .Returns(new Agent
            {
                Id = agentId,
                Username = "agentUsername",
                BrandId = brandId,
            },
                new Agent
                {
                    Id = Guid.Parse("6e51ebd9-6439-494e-ac6e-ee623a2f6293"),
                    Username = targetAgentName,
                    BrandId = brandId
                });

        var query = new AgentReduceAmountQuery(
            agentId,
            amount,
            remark,
            targetAgentName,
            AgentReduceTransactionType.Transfer,
            AccountType.Agent,
            "MONEY",
            userId);

        var actual = await sut.GetAsync(query).ConfigureAwait(false);

        Assert.False(actual.Valid);
        Assert.AreEqual(Resources.Agent.TargetAgentNotHomologue, actual.Message);
    }

    [Test]
    public async Task 代理轉帳代理_找不到目標代理()
    {
        var fakeAgentService = Substitute.For<IAgentService>();
        var fakeAgentWalletRepository = Substitute.For<IAgentWalletRepository>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerService = Substitute.For<IPlayerService>();
        var fakeJsonSerializationProvider = Substitute.For<IJsonSerializationProvider>();
        var fakeAdjustAgentAccountService = Substitute.For<IAdjustAgentAccountService>();
        var fakeAdjustTransferService = Substitute.For<IAdjustTransferService>();

        var sut = new AgentReduceAmountQueryService(
            fakeAgentService,
            fakeAgentWalletRepository,
            fakeCashServiceClient,
            fakePlayerService,
            fakeJsonSerializationProvider,
            fakeAdjustAgentAccountService, fakeAdjustTransferService);

        var agentId = Guid.Parse("5056ebbf-a975-42f1-9e97-b3299f0cab06");
        var amount = 168M;
        var userId = "2d26bf36-3f32-4874-994b-08da91f9a401";
        var playerDefaultWalletId = Guid.Parse("af5effa8-6352-4483-36cc-08dab0b34fcf");
        var remark = "testRemark";
        var targetAgentName = "1shaun";
        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        _ = fakeAgentService
            .GetAgentAsync(Arg.Any<Expression<Func<Agent, bool>>>())
            .Returns(new Agent
            {
                Id = agentId,
                Username = "agentUsername",
                BrandId = brandId,
            },
                default(Agent));

        var query = new AgentReduceAmountQuery(
            agentId,
            amount,
            remark,
            targetAgentName,
            AgentReduceTransactionType.Transfer,
            AccountType.Agent,
            "MONEY",
            userId);

        var actual = await sut.GetAsync(query).ConfigureAwait(false);

        Assert.False(actual.Valid);
        Assert.AreEqual(Resources.Agent.TargetAgentNotFound, actual.Message);
    }

    [Test]
    public async Task 代理轉帳代理成功()
    {
        var fakeAgentService = Substitute.For<IAgentService>();
        var fakeAgentWalletRepository = Substitute.For<IAgentWalletRepository>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerService = Substitute.For<IPlayerService>();
        var fakeJsonSerializationProvider = Substitute.For<IJsonSerializationProvider>();
        var fakeAdjustAgentAccountService = Substitute.For<IAdjustAgentAccountService>();
        var fakeAdjustTransferService = Substitute.For<IAdjustTransferService>();

        var sut = new AgentReduceAmountQueryService(
            fakeAgentService,
            fakeAgentWalletRepository,
            fakeCashServiceClient,
            fakePlayerService,
            fakeJsonSerializationProvider,
            fakeAdjustAgentAccountService, fakeAdjustTransferService);

        var agentId = Guid.Parse("5056ebbf-a975-42f1-9e97-b3299f0cab06");
        var agentUsername = "agentUsername";
        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var agentWalletId = Guid.Parse("c4427c4d-4822-48a0-bb05-08da80dc4545");
        var targetAgentName = "1shaun";
        var targetAgentWalletId = Guid.Parse("a99001ec-d437-4749-89be-08db8fff3332");
        var amount = 168M;
        var userId = "2d26bf36-3f32-4874-994b-08da91f9a401";
        var playerDefaultWalletId = Guid.Parse("af5effa8-6352-4483-36cc-08dab0b34fcf");
        var remark = "testRemark";

        _ = fakeAgentService
            .GetAgentAsync(Arg.Any<Expression<Func<Agent, bool>>>())
            .Returns(new Agent
            {
                Id = agentId,
                Username = agentUsername,
                BrandId = brandId,
                DefaultWalletId = agentWalletId
            },
                new Agent
                {
                    Id = Guid.Parse("6e51ebd9-6439-494e-ac6e-ee623a2f6293"),
                    Username = targetAgentName,
                    BrandId = brandId,
                    ParentAgentId = agentId,
                    DefaultWalletId = targetAgentWalletId
                });

        var query = new AgentReduceAmountQuery(
            agentId,
            amount,
            remark,
            targetAgentName,
            AgentReduceTransactionType.Transfer,
            AccountType.Agent,
            "MONEY",
            userId);

        var actual = await sut.GetAsync(query).ConfigureAwait(false);

        _ = fakeAdjustTransferService
            .Received(1)
            .CreateAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Is(AdjustTransferType.AgentToAgent),
                Arg.Is(agentUsername),
                Arg.Is(targetAgentName),
                Arg.Is(amount));

        Assert.True(actual.Valid);
    }

    [Test]
    public async Task 代理轉帳玩家_找不到目標玩家()
    {
        var fakeAgentService = Substitute.For<IAgentService>();
        var fakeAgentWalletRepository = Substitute.For<IAgentWalletRepository>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerService = Substitute.For<IPlayerService>();
        var fakeJsonSerializationProvider = Substitute.For<IJsonSerializationProvider>();
        var fakeAdjustAgentAccountService = Substitute.For<IAdjustAgentAccountService>();
        var fakeAdjustTransferService = Substitute.For<IAdjustTransferService>();

        var sut = new AgentReduceAmountQueryService(
            fakeAgentService,
            fakeAgentWalletRepository,
            fakeCashServiceClient,
            fakePlayerService,
            fakeJsonSerializationProvider,
            fakeAdjustAgentAccountService, fakeAdjustTransferService);

        var agentId = Guid.Parse("5056ebbf-a975-42f1-9e97-b3299f0cab06");
        var amount = 168M;
        var userId = "2d26bf36-3f32-4874-994b-08da91f9a401";
        var playerDefaultWalletId = Guid.Parse("af5effa8-6352-4483-36cc-08dab0b34fcf");
        var playerUsername = "shaun01";
        var remark = "testRemark";

        _ = fakeAgentService
            .GetAgentAsync(Arg.Any<Expression<Func<Agent, bool>>>())
            .Returns(new Agent
            {
                Id = agentId,
                Username = "agentUsername"
            });

        var query = new AgentReduceAmountQuery(
            agentId,
            amount,
            remark,
            playerUsername,
            AgentReduceTransactionType.Transfer,
            AccountType.Player,
            "MONEY",
            userId);

        var actual = await sut.GetAsync(query).ConfigureAwait(false);

        Assert.False(actual.Valid);
        Assert.AreEqual(Resources.Agent.TargetPlayerNotFound, actual.Message);
    }

    [Test]
    public async Task 代理轉帳玩家成功()
    {
        var fakeAgentService = Substitute.For<IAgentService>();
        var fakeAgentWalletRepository = Substitute.For<IAgentWalletRepository>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerService = Substitute.For<IPlayerService>();
        var fakeJsonSerializationProvider = Substitute.For<IJsonSerializationProvider>();
        var fakeAdjustAgentAccountService = Substitute.For<IAdjustAgentAccountService>();
        var fakeAdjustTransferService = Substitute.For<IAdjustTransferService>();

        var sut = new AgentReduceAmountQueryService(
            fakeAgentService,
            fakeAgentWalletRepository,
            fakeCashServiceClient,
            fakePlayerService,
            fakeJsonSerializationProvider,
            fakeAdjustAgentAccountService, fakeAdjustTransferService);

        var agentId = Guid.Parse("5056ebbf-a975-42f1-9e97-b3299f0cab06");
        var agentUsername = "agentUsername";
        var amount = 168M;
        var userId = "2d26bf36-3f32-4874-994b-08da91f9a401";
        var playerDefaultWalletId = Guid.Parse("af5effa8-6352-4483-36cc-08dab0b34fcf");
        var playerUsername = "shaun01";
        var remark = "testRemark";

        _ = fakeAgentService
            .GetAgentAsync(Arg.Any<Expression<Func<Agent, bool>>>())
            .Returns(new Agent
            {
                Id = agentId,
                Username = agentUsername
            });

        var player = new Player
        {
            Username = playerUsername,
            AgentId = agentId,
            DefaultWalletId = playerDefaultWalletId
        };

        _ = fakePlayerService
            .GetPlayerAsync(Arg.Any<Expression<Func<Player, bool>>>())
            .Returns(player);

        var transactionId = Guid.NewGuid();
        _ = fakeCashServiceClient
            .AdjustTransferAsync(Arg.Is(playerDefaultWalletId), Arg.Is(amount), Arg.Is(remark), Arg.Any<string>(), Arg.Is(Guid.Parse(userId)))
            .Returns(new TransactionResponse
            {
                TransactionId = transactionId,
                Balance = 60000M,
                LockedBalance = 0M,
            });

        var query = new AgentReduceAmountQuery(
            agentId,
            amount,
            remark,
            playerUsername,
            AgentReduceTransactionType.Transfer,
            AccountType.Player,
            "MONEY",
            userId);

        var actual = await sut.GetAsync(query).ConfigureAwait(false);

        _ = fakeAdjustTransferService
            .Received(1)
            .CreateAsync(
                Arg.Any<Guid>(),
                Arg.Is(transactionId),
                Arg.Is(AdjustTransferType.AgentToPlayer),
                Arg.Is(agentUsername),
                Arg.Is(playerUsername),
                Arg.Is(amount));

        Assert.True(actual.Valid);
    }

    [Test]
    public async Task 代理轉帳給非直屬玩家()
    {
        var fakeAgentService = Substitute.For<IAgentService>();
        var fakeAgentWalletRepository = Substitute.For<IAgentWalletRepository>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerService = Substitute.For<IPlayerService>();
        var fakeJsonSerializationProvider = Substitute.For<IJsonSerializationProvider>();
        var fakeAdjustAgentAccountService = Substitute.For<IAdjustAgentAccountService>();
        var fakeAdjustTransferService = Substitute.For<IAdjustTransferService>();

        var sut = new AgentReduceAmountQueryService(
            fakeAgentService,
            fakeAgentWalletRepository,
            fakeCashServiceClient,
            fakePlayerService,
            fakeJsonSerializationProvider,
            fakeAdjustAgentAccountService, fakeAdjustTransferService);

        var agentId = Guid.Parse("5056ebbf-a975-42f1-9e97-b3299f0cab06");
        var amount = 168M;
        var userId = "2d26bf36-3f32-4874-994b-08da91f9a401";
        var playerDefaultWalletId = Guid.Parse("af5effa8-6352-4483-36cc-08dab0b34fcf");
        var playerUsername = "shaun01";
        var remark = "testRemark";

        _ = fakeAgentService
            .GetAgentAsync(Arg.Any<Expression<Func<Agent, bool>>>())
            .Returns(new Agent
            {
                Id = agentId,
                Username = "agentUsername"
            });

        var player = new Player
        {
            Username = playerUsername,
            AgentId = null,
            DefaultWalletId = playerDefaultWalletId
        };

        _ = fakePlayerService
            .GetPlayerAsync(Arg.Any<Expression<Func<Player, bool>>>())
            .Returns(player);

        var query = new AgentReduceAmountQuery(
            agentId,
            amount,
            remark,
            playerUsername,
            AgentReduceTransactionType.Transfer,
            AccountType.Player,
            "MONEY",
            userId);

        var actual = await sut.GetAsync(query).ConfigureAwait(false);

        Assert.False(actual.Valid);
        Assert.AreEqual(Resources.Agent.TargetPlayerNotUnderAgent, actual.Message);
    }

    [Test]
    public async Task 找不到代理()
    {
        var fakeAgentService = Substitute.For<IAgentService>();
        var fakeAgentWalletRepository = Substitute.For<IAgentWalletRepository>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakePlayerService = Substitute.For<IPlayerService>();
        var fakeJsonSerializationProvider = Substitute.For<IJsonSerializationProvider>();
        var fakeAdjustAgentAccountService = Substitute.For<IAdjustAgentAccountService>();
        var fakeAdjustTransferService = Substitute.For<IAdjustTransferService>();

        var sut = new AgentReduceAmountQueryService(
            fakeAgentService,
            fakeAgentWalletRepository,
            fakeCashServiceClient,
            fakePlayerService,
            fakeJsonSerializationProvider,
            fakeAdjustAgentAccountService, fakeAdjustTransferService);

        var agentId = Guid.Parse("5056ebbf-a975-42f1-9e97-b3299f0cab06");
        var amount = 168M;
        var userId = "2d26bf36-3f32-4874-994b-08da91f9a401";
        var playerUsername = "shaun01";
        var remark = "testRemark";

        var query = new AgentReduceAmountQuery(
            agentId,
            amount,
            remark,
            playerUsername,
            AgentReduceTransactionType.Transfer,
            AccountType.Player,
            "MONEY",
            userId);

        var actual = await sut.GetAsync(query).ConfigureAwait(false);

        Assert.False(actual.Valid);
        Assert.AreEqual(Resources.Agent.NotFound, actual.Message);
    }
}
