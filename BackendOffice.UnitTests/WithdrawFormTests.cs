using System.Collections.Immutable;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework.Legacy;
using Stateless;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.BackOffice.Application.Commands.Withdraw;
using TGS.Services.BackOffice.Application.Queries.Withdraw;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using TGS.Services.BackOffice.ApplicationServices.Payment;
using TGS.Services.BackOffice.ApplicationServices.Players;
using TGS.Services.BackOffice.Infrastructure.DataService;
using UserType = Tgs.Payment.Clients.UserType;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class WithdrawFormTests
{
    [Test]
    public async Task ReviewWithdrawFormCommandService_審核提款單()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new ReviewWithdrawFormCommandService(
            fakePaymentServiceClient);

        var formId = Guid.NewGuid();
        var brandPaymentId = Guid.NewGuid();
        var brandId = Guid.NewGuid();

        var command = new ReviewWithdrawFormCommand(
            brandId,
            formId,
            brandPaymentId,
            ReviewState.Approve,
            null,
            "playerRemark",
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
                Arg.Any<string>(),
                Arg.Is("playerRemark"));
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
    public async Task User_Can_Get_Untreated_WithdrawForms_Count()
    {
        var moqPaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new CountUntreatedWithdrawFormsQueryService(
            moqPaymentServiceClient,
            NullLogger<CountUntreatedWithdrawFormsQueryService>.Instance);

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