using System.Collections.Immutable;
using System.Linq.Expressions;
using NSubstitute;
using Stateless;
using TGS.Core.DataServices.Tgs.Models;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using TGS.Services.BackOffice.Application.Commands.Withdraw;
using TGS.Services.BackOffice.Application.Queries.Withdraw;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using TGS.Services.BackOffice.ApplicationServices.Payment;
using TGS.Services.BackOffice.ApplicationServices.Players;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;
using TGS.Services.WalletService;
using Tgs.ServiceUtility;
using UserType = Tgs.Payment.Clients.UserType;
using NUnit.Framework.Legacy;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class WithdrawFormTests
{
    [Test]
    public async Task Can_Approve_Withdraw_Form()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeWalletExchangeService = Substitute.For<IWalletExchangeService>();

        var sut = new ReviewWithdrawFormCommandService(
            fakePaymentServiceClient,
            fakeWalletExchangeService);

        var formId = Guid.NewGuid();
        var brandPaymentId = Guid.NewGuid();
        var brandId = Guid.NewGuid();

        var command = new ReviewWithdrawFormCommand(
            brandId,
            formId,
            brandPaymentId,
            ReviewState.Approve,
            null,
            "93a723b2-9ebb-4607-ee8d-08d978fba75f",
            "reviewUserName");

        _ = fakePaymentServiceClient
            .GetWithdrawFormAsync(Arg.Is(brandId), Arg.Is(formId))
            .Returns(new WithdrawFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.WaitApprove,
                Amount = 1000M
            });

        var fakeFsm = new StateMachine<PaymentFlowState, PaymentTrigger>(PaymentFlowState.WaitApprove);
        _ = fakeFsm.Configure(PaymentFlowState.WaitApprove)
            .Permit(PaymentTrigger.Approve, PaymentFlowState.Approved);

        _ = fakePaymentServiceClient
            .CreateWithdrawStateMachine(Arg.Is(PaymentFlowState.WaitApprove))
            .Returns(fakeFsm);

        await sut.ExecuteAsync(command);

        _ = fakePaymentServiceClient.Received(1)
            .ReviewWithdrawFormAsync(
                Arg.Is(formId),
                Arg.Is(brandPaymentId),
                Arg.Is(ReviewState.Approve),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>());
    }

    [Test]
    public async Task Can_Get_Agent_RealName()
    {
        var fakeAgentService = Substitute.For<IAgentService>();

        var agentId = Guid.Parse("9fbc1b81-44ee-4f00-ba12-a13170c01024");

        var agent = new Agent
        {
            RealName = "agentRealName",
            Id = agentId
        };

        _ = fakeAgentService.GetAgentAsync(Arg.Any<Expression<Func<Agent, bool>>>())
            .Returns(agent);

        var sut = new GetAgentRealNameQueryService(fakeAgentService);

        var actual = await sut.GetAsync(new(agentId));

        ClassicAssert.AreEqual("agentRealName", actual);
    }

    [Test]
    public async Task Can_Get_Player_RealName()
    {
        var fakePlayerService = Substitute.For<IPlayerServiceClient>();

        var playerCode = "4ucmqplxb029";

        var playerProperties = new Dictionary<string, string> { { "RealName", "playerRealName" } };

        _ = fakePlayerService.GetPlayerPropertiesAsync(playerCode)
            .Returns(playerProperties.ToImmutableDictionary());

        var sut = new GetPlayerRealNameQueryService(fakePlayerService);

        var actual = await sut.GetAsync(new(playerCode));

        ClassicAssert.AreEqual("playerRealName", actual);
    }

    [Test]
    public async Task Can_Get_Withdraw_Form_Agent_RealName()
    {
        var fakeGetAgentService = Substitute.For<IGetService<GetAgentRealNameQuery, string>>();
        var fakeGetPlayerPropertiesService = Substitute.For<IGetService<GetPlayerRealNameQuery, string>>();

        var agentId = "9fbc1b81-44ee-4f00-ba12-a13170c01024";
        _ = fakeGetAgentService
            .GetAsync(new(Guid.Parse(agentId)))
            .Returns("DeveloperExperence");

        var sut = new GetWithdrawFormRealNameQueryService(fakeGetAgentService, fakeGetPlayerPropertiesService);

        var actual = await sut.GetAsync(new(UserType.Agent, agentId));

        ClassicAssert.AreEqual("DeveloperExperence", actual);
    }

    [Test]
    public async Task Can_Get_Withdraw_Form_Player_RealName()
    {
        var fakeGetAgentService = Substitute.For<IGetService<GetAgentRealNameQuery, string>>();
        var fakeGetPlayerPropertiesService = Substitute.For<IGetService<GetPlayerRealNameQuery, string>>();

        var playerCode = "4ucmqplxb029";
        _ = fakeGetPlayerPropertiesService
            .GetAsync(new(playerCode))
            .Returns("DeveloperExperence");

        var sut = new GetWithdrawFormRealNameQueryService(fakeGetAgentService, fakeGetPlayerPropertiesService);

        var actual = await sut.GetAsync(new(UserType.Player, playerCode));

        ClassicAssert.AreEqual("DeveloperExperence", actual);
    }

    [Test]
    public async Task Can_Reject_Withdraw_Form_Without_BrandPaymentId()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeWalletExchangeService = Substitute.For<IWalletExchangeService>();

        var sut = new ReviewWithdrawFormCommandService(
            fakePaymentServiceClient,
            fakeWalletExchangeService);

        var formId = Guid.NewGuid();
        var brandId = Guid.NewGuid();

        var command = new ReviewWithdrawFormCommand(
            brandId,
            formId,
            null,
            ReviewState.Reject,
            null,
            "93a723b2-9ebb-4607-ee8d-08d978fba75f",
            "reviewUserName");

        _ = fakePaymentServiceClient
            .GetWithdrawFormAsync(Arg.Is(brandId), Arg.Is(formId))
            .Returns(new WithdrawFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.Approved,
                Amount = 1000M
            });

        var fakeFsm = new StateMachine<PaymentFlowState, PaymentTrigger>(PaymentFlowState.Approved);
        _ = fakeFsm.Configure(PaymentFlowState.Approved)
            .Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

        _ = fakePaymentServiceClient
            .CreateWithdrawStateMachine(Arg.Is(PaymentFlowState.Approved))
            .Returns(fakeFsm);

        await sut.ExecuteAsync(command);

        _ = fakePaymentServiceClient.Received(1)
            .ReviewWithdrawFormAsync(
                Arg.Is(formId),
                Arg.Is(default(Guid?)),
                Arg.Is(ReviewState.Reject),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>());

        _ = fakeWalletExchangeService.Received(1)
            .RollbackWithdrawAsync(
                Arg.Is(brandId),
                Arg.Any<UserType>(),
                Arg.Any<string>(),
                Arg.Is(1000M),
                Arg.Any<string>(),
                Arg.Any<Guid>());
    }

    [Test]
    public async Task Commit_Withdraw_By_Agent_Will_Do_Nothing()
    {
        var fakePlayerWalletExchangeService = Substitute.For<IPlayerWalletExchangeService>();
        var fakeAgentService = Substitute.For<IAgentService>();
        var fakeAgentWalletRepository = Substitute.For<IAgentWalletRepository>();

        var sut = new WalletExchangeService(
            fakePlayerWalletExchangeService,
            fakeAgentService,
            fakeAgentWalletRepository);

        var brandId = Guid.NewGuid();
        var userType = UserType.Agent;
        var agentId = Guid.NewGuid();
        var transactionType = TransactionTypes.CancelWithdrawal;
        var amount = 1000M;
        var userId = Guid.NewGuid();

        _ = fakeAgentService.GetAgentAsync(
                Arg.Any<Expression<Func<Agent, bool>>>(),
                Arg.Any<Func<IQueryable<Agent>, IQueryable<Agent>>>())
            .Returns(new Agent
            {
                Id = agentId,
                DefaultWalletId = Guid.NewGuid()
            });

        await sut.CommitWithdrawAsync(
            brandId,
            userType,
            agentId.ToString(),
            amount,
            amount,
            userId.ToString(),
            Guid.NewGuid());

        _ = fakeAgentWalletRepository.Received(0)
            .ExchangeAsync(
                Arg.Is(brandId),
                Arg.Any<Guid>(),
                Arg.Is(agentId),
                Arg.Is(transactionType),
                Arg.Is(amount),
                Arg.Any<string>());
    }

    [Test]
    public async Task Commit_Withdraw_By_Player_Should_Call_PlayerWalletExchangeService()
    {
        var fakePlayerWalletExchangeService = Substitute.For<IPlayerWalletExchangeService>();
        var fakeAgentService = Substitute.For<IAgentService>();
        var fakeAgentWalletRepository = Substitute.For<IAgentWalletRepository>();

        var sut = new WalletExchangeService(
            fakePlayerWalletExchangeService,
            fakeAgentService,
            fakeAgentWalletRepository);

        var brandId = Guid.NewGuid();
        var userType = UserType.Player;
        var playerCode = "ThisIsPlayerCode";
        var amount = 1000M;
        var userId = Guid.NewGuid();

        await sut.CommitWithdrawAsync(
            brandId,
            userType,
            playerCode,
            amount,
            amount,
            userId.ToString(),
            Guid.NewGuid());

        _ = fakePlayerWalletExchangeService.Received(1)
            .CommitWithdrawalAsync(
                Arg.Is(playerCode),
                Arg.Is(amount),
                Arg.Is(amount),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Guid?>());
    }

    [Test]
    public async Task Manager_Can_Get_The_WithdrawForm_Detail()
    {
        var moqPaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new GetWithdrawFormDetailQueryService(moqPaymentServiceClient);

        var formId = Guid.NewGuid();
        var brandId = Guid.NewGuid();

        var query = new GetWithdrawFormDetailQuery(formId, brandId);

        _ = await sut.GetAsync(query);

        _ = moqPaymentServiceClient.Received(1).GetWithdrawFormAsync(
            Arg.Is<Guid>(x => x == brandId),
            Arg.Is<Guid>(x => x == formId),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Manager_Can_Paid_Manual_WithdrawForm()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeWalletExchangeService = Substitute.For<IWalletExchangeService>();

        var sut = new PaidWithdrawFormCommandService(fakePaymentServiceClient, fakeWalletExchangeService);

        var formId = Guid.NewGuid();
        var brandPaymentId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var command = new PaidWithdrawFormCommand(
            brandId,
            formId,
            "93a723b2-9ebb-4607-ee8d-08d978fba75f",
            "reviewUserName");

        _ = fakePaymentServiceClient.CreateWithdrawStateMachine(Arg.Any<PaymentFlowState>())
            .ReturnsForAnyArgs(callInfo =>
            {
                var initialState = callInfo.Arg<PaymentFlowState>();

                var fsm = new StateMachine<PaymentFlowState, PaymentTrigger>(initialState);

                _ = fsm.Configure(PaymentFlowState.Init)
                    .Permit(PaymentTrigger.WithdrawCreate, PaymentFlowState.WaitApprove)
                    .Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

                _ = fsm.Configure(PaymentFlowState.WaitApprove)
                    .Permit(PaymentTrigger.Approve, PaymentFlowState.Approved)
                    .Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

                _ = fsm.Configure(PaymentFlowState.Approved)
                    .Permit(PaymentTrigger.PaymentWithdrawCompleted, PaymentFlowState.Paid)
                    .Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

                return fsm;
            });

        _ = fakePaymentServiceClient.GetWithdrawFormAsync(
                Arg.Is(brandId),
                Arg.Is(formId))
            .Returns(new WithdrawFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.Approved,
                Amount = 1000M
            });

        await sut.ExecuteAsync(command);

        _ = fakePaymentServiceClient.Received(1)
            .SubmitWithdrawFormStatusAsync(
                Arg.Is(formId),
                Arg.Is(PaymentFlowState.Paid),
                Arg.Any<string>(),
                Arg.Any<decimal?>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>());

        _ = fakeWalletExchangeService.Received(1)
            .CommitWithdrawAsync(
                Arg.Is(brandId),
                Arg.Any<UserType>(),
                Arg.Any<string>(),
                Arg.Is(1000M),
                Arg.Is(1000M),
                Arg.Any<string>(),
                Arg.Any<Guid>());
    }

    [Test]
    public async Task Manager_Can_Review_WithdrawForm()
    {
        var moqPaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var moqWalletExchangeService = Substitute.For<IWalletExchangeService>();

        var sut = new ReviewWithdrawFormCommandService(
            moqPaymentServiceClient,
            moqWalletExchangeService);

        var formId = Guid.NewGuid();
        var brandPaymentId = Guid.NewGuid();
        var brandId = Guid.NewGuid();

        var command = new ReviewWithdrawFormCommand(
            brandId,
            formId,
            brandPaymentId,
            ReviewState.Approve,
            null,
            "93a723b2-9ebb-4607-ee8d-08d978fba75f",
            "reviewUserName");

        _ = moqPaymentServiceClient.GetWithdrawFormAsync(
                Arg.Is<Guid>(x => x == brandId),
                Arg.Is<Guid>(x => x == formId),
                Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult<WithdrawFormInfo?>(
                new()
                {
                    Id = formId,
                    BrandId = brandId,
                    Status = PaymentFlowState.WaitApprove
                }));

        var fakeFsm = new StateMachine<PaymentFlowState, PaymentTrigger>(PaymentFlowState.WaitApprove);

        _ = fakeFsm.Configure(PaymentFlowState.WaitApprove)
            .Permit(PaymentTrigger.Approve, PaymentFlowState.Approved);

        _ = moqPaymentServiceClient
            .CreateWithdrawStateMachine(
                Arg.Is<PaymentFlowState>(x => x == PaymentFlowState.WaitApprove))
            .Returns(fakeFsm);

        await sut.ExecuteAsync(command);

        _ = moqPaymentServiceClient.Received(1).ReviewWithdrawFormAsync(
            Arg.Is<Guid>(x => x == formId),
            Arg.Is<Guid>(x => x == brandPaymentId),
            Arg.Is<ReviewState>(x => x == ReviewState.Approve),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Rollback_Withdraw_By_Agent_Should_Call_AgentWalletRepository()
    {
        var fakePlayerWalletExchangeService = Substitute.For<IPlayerWalletExchangeService>();
        var fakeAgentService = Substitute.For<IAgentService>();
        var fakeAgentWalletRepository = Substitute.For<IAgentWalletRepository>();

        var sut = new WalletExchangeService(
            fakePlayerWalletExchangeService,
            fakeAgentService,
            fakeAgentWalletRepository);

        var brandId = Guid.NewGuid();
        var userType = UserType.Agent;
        var agentId = Guid.NewGuid();
        var transactionType = TransactionTypes.CancelWithdrawal;
        var amount = 1000M;
        var userId = Guid.NewGuid();

        _ = fakeAgentService.GetAgentAsync(
                Arg.Any<Expression<Func<Agent, bool>>>(),
                Arg.Any<Func<IQueryable<Agent>, IQueryable<Agent>>>())
            .Returns(new Agent
            {
                Id = agentId,
                DefaultWalletId = Guid.NewGuid()
            });

        await sut.RollbackWithdrawAsync(
            brandId,
            userType,
            agentId.ToString(),
            amount,
            userId.ToString(),
            Guid.NewGuid());

        _ = fakeAgentWalletRepository.Received(1)
            .ExchangeAsync(
                Arg.Is(brandId),
                Arg.Any<Guid>(),
                Arg.Is(agentId),
                Arg.Is(transactionType),
                Arg.Is(amount),
                Arg.Any<string>());
    }

    [Test]
    public async Task Rollback_Withdraw_By_Player_Should_Call_PlayerWalletExchangeService()
    {
        var fakePlayerWalletExchangeService = Substitute.For<IPlayerWalletExchangeService>();
        var fakeAgentService = Substitute.For<IAgentService>();
        var fakeAgentWalletRepository = Substitute.For<IAgentWalletRepository>();

        var sut = new WalletExchangeService(
            fakePlayerWalletExchangeService,
            fakeAgentService,
            fakeAgentWalletRepository);

        var brandId = Guid.NewGuid();
        var userType = UserType.Player;
        var playerCode = "ThisIsPlayerCode";
        var amount = 1000M;
        var userId = Guid.NewGuid();

        await sut.RollbackWithdrawAsync(
            brandId,
            userType,
            playerCode,
            amount,
            userId.ToString(),
            Guid.NewGuid());

        _ = fakePlayerWalletExchangeService.Received(1)
            .RollbackWithdrawalAsync(
                Arg.Is(playerCode),
                Arg.Is(amount),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Guid?>());
    }

    [Test]
    public async Task User_Can_Get_Untreated_WithdrawForms_Count()
    {
        var moqPaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new CountUntreatedWithdrawFormsQueryService(
            moqPaymentServiceClient);

        var query = new CountUntreatedWithdrawFormsQuery(Guid.NewGuid());

        _ = await sut.GetAsync(query);

        _ = moqPaymentServiceClient.Received(1).CountUntreatedWithdrawFormsAsync(
            Arg.Is<Guid>(x => x == query.BrandId),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task User_Can_List_WithdrawForm()
    {
        var moqPaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new WithdrawFormQueryService(moqPaymentServiceClient);

        var query = new WithdrawFormQuery(
            Guid.NewGuid(),
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            new());

        _ = await sut.GetAsync(query, default);

        _ = moqPaymentServiceClient.QueryWithdrawFormsAsync(
            Arg.Any<WithdrawFormQueryConditions>(),
            Arg.Any<Paging>(),
            Arg.Any<CancellationToken>());
    }
}