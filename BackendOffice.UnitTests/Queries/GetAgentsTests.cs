using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.CashService.Client;
using TGS.Services.BackOffice.Application.Models.Agent;
using TGS.Services.BackOffice.Application.Models.Language;
using TGS.Services.BackOffice.Application.Queries.AgentHandlers;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Tests.BackendOffice.UnitTests.Queries
{
    public class GetAgentsTests
    {
        [Test]
        public void Get_Agents()
        {
            var fakeAgentService = Substitute.For<IAgentService>();
            var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
            var fakeAgentClient = Substitute.For<IAgentClient>();
            var fakeHttpContextMock = Substitute.For<IHttpContextAccessor>();

            var agentModels = new AgentModel[] { new AgentModel { Id = Guid.NewGuid() } };

            _ = fakeAgentService.GetAgentsAsync(Arg.Any<GetAgentsRequest>()).Returns((agentModels, 1));

            _ = fakeAgentClient.GetAgentInfoAsync(Arg.Any<GetAgentInfoRequest>())
                .Returns(CreateAgentInfoModelFixture().Create<AgentInfoModel>());

            var sut = new GetAgentsQueryHandler(fakeAgentService,
                fakeCashServiceClient,
                fakeAgentClient,
                fakeHttpContextMock);

            var request = new GetAgentsRequest { };

            _ = sut.Handle(request, default);

            fakeAgentClient.ReceivedWithAnyArgs(1).GetAgentInfoAsync(null, default);
            fakeCashServiceClient.ReceivedWithAnyArgs(1).GetBalanceAsync(default, default);
        }

        private static Fixture CreateAgentInfoModelFixture()
        {
            var fixture = new Fixture();
            _ = fixture.Create<AgentInfoModel>();
            return fixture;
        }
    }
}
