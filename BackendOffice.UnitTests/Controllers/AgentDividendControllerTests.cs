using AutoFixture;
using NSubstitute;
using Tgs.Agent.Dividend.Client;
using Tgs.Agent.Dividend.Client.Models;
using TGS.Apis.BackendOffice.Controllers.Agent;
using TGS.Apis.BackendOffice.Controllers.Messages.AgentContract;
using TGS.Apis.BackendOffice.Controllers.Messages.AgentDividend;
using Tgs.BrandCertificate.Clients;
using TGS.Services.BackOffice.Domain.Enums;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class AgentDividendControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Can_Get_Agent_Dividend_Records()
    {
        var fakeAgentDividendClient = Substitute.For<IAgentDividendClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

        var controller =
            new AgentDividendController(fakeAgentDividendClient, fakeBrandServiceClient)
            {
                ControllerContext = { HttpContext = HttpContextMock }
            };

        var agentDividendRecords = new Fixture().CreateMany<AgentDividendRecordModel>(100);

        _ = fakeAgentDividendClient.GetAgentDividendRecordsAsync(Arg.Any<GetAgentDividendRecords>(), Arg.Any<CancellationToken>())
            .Returns(agentDividendRecords.ToAsyncEnumerable());

        var request = new GetAgentDividendRecordsRequest
        {
            BrandId = Guid.NewGuid(),
            Status = AgentDividendDistributeStatus.ReadyToApprove,
            From = DateTime.UtcNow,
            To = DateTime.UtcNow,
            Page = 1,
            Rows = 15,
            ShowAll = false,
            SortBy = "agentUsername"
        };

        var response = await controller.GetAgentDividendRecordsAsync(request);

        Assert.That(response.Valid, Is.True);
        Assert.That(response.TotalCount, Is.EqualTo(100));
        Assert.That(response.Items.Length, Is.EqualTo(15));

        _ = fakeAgentDividendClient.ReceivedWithAnyArgs(1).GetAgentDividendRecordsAsync(null!, default);
    }
}