using MediatR;
using NSubstitute;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using TGS.Apis.BackendOffice.Controllers.Agent;
using Tgs.Report.Client;
using Tgs.CashService.Client;
using TGS.Services.BackOffice.Application.Models.ReportAgent;
using AutoFixture;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[Parallelizable]
public class AgentControllerTests : BaseTests
{
    [Test]
    public async Task Clear_Negative_Commission()
    {
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeMediator = Substitute.For<IMediator>();

        var sut = new AgentController(fakeMediator)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var agentId = Guid.NewGuid();

        _ = await sut.ClearUnDistributedCommissionAsync(fakeAgentClient, agentId);

        await fakeAgentClient
            .Received(1)
            .ClearAgentNegativeCommissionAsync(Arg.Is<ClearAgentNegativeCommissionRequest>(x
                => agentId == x.AgentId &&
                Guid.Parse("e4ecfa98-b373-4123-87e0-cb9e883d0691") == x.ClearByUserId));
    }

    [Test]
    public async Task Get_Agent_Account_Changes()
    {
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakeMediator = Substitute.For<IMediator>();

        var sut = new AgentController(fakeMediator)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        var agentId = Guid.NewGuid();

        _ = fakeAgentClient.GetAgentInfoAsync(Arg.Any<GetAgentInfoRequest>())
            .Returns(CreateAgentInfoFixture(agentId).Create<AgentInfoModel>());

        _ = fakeCashServiceClient.QueryTransactionLogsAsync(Arg.Any<Guid>(),
                Arg.Any<DateTime>(),
                Arg.Any<DateTime>(),
                Arg.Any<IEnumerable<int>>(),
                Arg.Any<PagingConditions>(),
                Arg.Any<CancellationToken>())
            .Returns(new Tgs.CashService.Client.PagingResponse<TransactionLog>(new[]
                {
                    CreateAgentTransactionFixture().Create<TransactionLog>()
                },
                1,
                10));

        var request = new GetAgentAccountChangesReportRequest { };

        _ = await sut.GetAgentAccountChangesAsync(fakeCashServiceClient, fakeAgentClient, request);

        _ = await fakeAgentClient
            .ReceivedWithAnyArgs(1)
            .GetAgentInfoAsync(null, default);
    }

    private static Fixture CreateAgentTransactionFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<TransactionLog>(u => u);

        return fixture;
    }

    private static Fixture CreateAgentInfoFixture(Guid agentId)
    {
        var fixture = new Fixture();

        fixture.Customize<AgentInfoModel>(u => u
            .With(s => s.Id, agentId)
            .With(s => s.WalletId, Guid.NewGuid()));

        return fixture;
    }


    [SetUp]
    public override void Setup() => base.Setup();
}