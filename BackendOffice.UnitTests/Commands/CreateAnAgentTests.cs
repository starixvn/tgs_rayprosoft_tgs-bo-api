using System.Linq.Expressions;
using System.Security.Claims;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.BrandCertificate.Clients;
using Tgs.CashService.Client;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Commands.AgentHandlers;
using TGS.Services.BackOffice.Application.Models.Agent;
using TGS.Services.BackOffice.Application.Queries.AgentHandlers;
using TGS.Services.BackOffice.Infrastructure.DataService;
using CreateAgentRequest = TGS.Services.BackOffice.Application.Models.Agent.CreateAgentRequest;

namespace TGS.Tests.BackendOffice.UnitTests.Commands;

public class CreateAnAgentTests
{
    [Test]
    public void Create_An_Agent()
    {
        var fakeAgentService = Substitute.For<IAgentService>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeHttpContextMock = Substitute.For<IHttpContextAccessor>();
        var httpContextMock = Substitute.For<HttpContext>();

        _ = httpContextMock.User.Returns(GetUserClaimsPrincipal());

        fakeHttpContextMock.HttpContext = httpContextMock;

        var brandId = Guid.NewGuid();
        var parentAgentId = Guid.NewGuid();

        var agentModels = new AgentModel[] { new() { Id = Guid.NewGuid() } };

        _ = fakeAgentService.GetAgentsAsync(Arg.Any<GetAgentsRequest>()).Returns((agentModels, 1));

        _ = fakeAgentClient.GetAgentInfoAsync(Arg.Any<GetAgentInfoRequest>())
            .Returns(CreateAgentInfoModelFixture().Create<AgentInfoModel>());

        _ = fakeBrandServiceClient.GetAllBrandCurrenciesAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { new BrandCurrencyInfo(brandId, "CNY") }.ToAsyncEnumerable());

        _ = fakeAgentService.GetAgentAsync(Arg.Any<Expression<Func<Agent, bool>>>())
            .Returns(new Agent
            {
                Id = parentAgentId,
                DividendPercentage = 50,
                Brand = new Brand { AgentDividendType = AgentDividendType.OneLayer }
            });

        var sut = new CreateAgentHandler(
            fakeAgentService,
            fakeAgentClient,
            fakeBrandServiceClient,
            fakeHttpContextMock);

        var request = new CreateAgentRequest
        {
            BrandId = brandId,
            Username = "username",
            ParentUsername = "parentUsername",
            DividendPercentage = 40,
            Password = "pwd",
            Remarks = ""
        };

        _ = sut.Handle(request, default);

        _ = fakeAgentClient.ReceivedWithAnyArgs(1).CreateAgentAsync(null!);

        _ = fakeAgentClient.Received(1)
            .CreateAgentAsync(
                Arg.Is<Tgs.Agent.Client.Models.CreateAgentRequest>(x => x.ParentAgentId == parentAgentId));
    }

    private static Fixture CreateAgentInfoModelFixture()
    {
        var fixture = new Fixture();
        _ = fixture.Create<AgentInfoModel>();
        return fixture;
    }

    protected static ClaimsPrincipal GetUserClaimsPrincipal(
        string username = "admin.enabled",
        string roleName = "Admin",
        RoleType roleType = RoleType.Admin,
        string id = "e4ecfa98-b373-4123-87e0-cb9e883d0691")
        => new(new[]
        {
            new ClaimsIdentity(new[]
            {
                new Claim(Consts.BackendOffice.UserInfo.UserIdKey, id),
                new Claim(Consts.BackendOffice.UserInfo.UsernameKey, username),
                new Claim(Consts.BackendOffice.UserInfo.RolenameKey, roleName),
                new Claim(Consts.BackendOffice.UserInfo.PortalKey, roleType.ToString())
            })
        });

}