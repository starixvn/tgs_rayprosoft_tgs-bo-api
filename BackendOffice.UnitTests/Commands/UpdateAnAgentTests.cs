using System.Security.Claims;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.BrandCertificate.Clients;
using TGS.Core.Models;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using AgentDividendType = Tgs.BrandCertificate.AgentDividendType;
using AgentType = Tgs.Agent.Client.Models.Enums.AgentType;

namespace TGS.Tests.BackendOffice.UnitTests.Commands;

public class UpdateAnAgentTests
{
    [Test]
    public void Update_One_Layer_Agent()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeHttpContextMock = Substitute.For<IHttpContextAccessor>();
        var httpContextMock = Substitute.For<HttpContext>();

        _ = httpContextMock.User.Returns(GetUserClaimsPrincipal());

        fakeHttpContextMock.HttpContext = httpContextMock;

        var agentId = Guid.NewGuid();
        var username = "username";
        var parentUsername = "parentUsername";

        _ = fakeAgentClient.GetAgentInfoAsync(Arg.Any<GetAgentInfoRequest>())
            .Returns(CreateAgentInfoModelFixture(username).Create<AgentInfoModel>());

        _ = fakeAgentClient.GetAgentInfoByUsernameAsync(Arg.Any<GetAgentInfoByUsernameRequest>())
            .Returns(CreateAgentInfoModelFixture(parentUsername).Create<AgentInfoModel>());

        _ = fakeBrandServiceClient.GetBrandFullInfoAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(CreateBrandFullInfoFixture().Create<BrandFullInfo>());

        var sut = new SetAgentOneLayerCommandService(
            fakeAgentClient,
            fakeBrandServiceClient);

        var request = new SetAgentOneLayerCommand(agentId, parentUsername, 10, AgentType.Cash, "test");

        _ = sut.GetAsync(request, default);

        _ = fakeAgentClient.ReceivedWithAnyArgs(1).SetOneLayerAgentInfoAsync(null!);
    }

    [Test]
    public void Set_One_Layer_Agent()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeHttpContextMock = Substitute.For<IHttpContextAccessor>();
        var httpContextMock = Substitute.For<HttpContext>();

        _ = httpContextMock.User.Returns(GetUserClaimsPrincipal());

        fakeHttpContextMock.HttpContext = httpContextMock;

        var agentId = Guid.NewGuid();
        var username = "username";
        var parentUsername = "parentUsername";

        var brandInfo = CreateBrandFullInfoFixture(AgentDividendType.OneLayer).Create<BrandFullInfo>();

        _ = fakeAgentClient.GetAgentInfoAsync(Arg.Any<GetAgentInfoRequest>())
            .Returns(CreateAgentInfoModelFixture(username).Create<AgentInfoModel>());

        _ = fakeAgentClient.GetAgentInfoByUsernameAsync(Arg.Any<GetAgentInfoByUsernameRequest>())
            .Returns(CreateAgentInfoModelFixture(parentUsername).Create<AgentInfoModel>());

        _ = fakeBrandServiceClient.GetBrandFullInfoAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(brandInfo);

        var sut = new SetAgentOneLayerCommandService(
            fakeAgentClient,
            fakeBrandServiceClient);

        var request = new SetAgentOneLayerCommand(agentId, parentUsername, 10, AgentType.Cash, "test");

        _ = sut.GetAsync(request, default);

        _ = fakeAgentClient.ReceivedWithAnyArgs(1).SetOneLayerAgentInfoAsync(null!);
    }

    [Test]
    public async Task Can_Not_Set_One_Layer_Agent_When_Agent_Is_Null()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeHttpContextMock = Substitute.For<IHttpContextAccessor>();
        var httpContextMock = Substitute.For<HttpContext>();

        _ = httpContextMock.User.Returns(GetUserClaimsPrincipal());

        fakeHttpContextMock.HttpContext = httpContextMock;

        var agentId = Guid.NewGuid();
        var parentUsername = "parentUsername";

        _ = fakeAgentClient.GetAgentInfoAsync(Arg.Any<GetAgentInfoRequest>())
            .Returns((AgentInfoModel?)null);

        var sut = new SetAgentOneLayerCommandService(
            fakeAgentClient,
            fakeBrandServiceClient);

        var request = new SetAgentOneLayerCommand(agentId, parentUsername, 10, AgentType.Cash, "test");

        var response = await sut.GetAsync(request, default);

        Assert.That(response.Valid, Is.EqualTo(false));
        Assert.That(response.Message, Is.EqualTo(Services.BackOffice.Application.Resources.Agent.NotFound));

        _ = fakeAgentClient.ReceivedWithAnyArgs(0).SetOneLayerAgentInfoAsync(null!);
    }

    [Test]
    public async Task Can_Not_Set_One_Layer_Agent_When_Username_Same_With_Parent_Username()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeHttpContextMock = Substitute.For<IHttpContextAccessor>();
        var httpContextMock = Substitute.For<HttpContext>();

        _ = httpContextMock.User.Returns(GetUserClaimsPrincipal());

        fakeHttpContextMock.HttpContext = httpContextMock;

        var agentId = Guid.NewGuid();
        var parentUsername = "parentUsername";

        var brandInfo = CreateBrandFullInfoFixture(AgentDividendType.OneLayer).Create<BrandFullInfo>();

        _ = fakeAgentClient.GetAgentInfoAsync(Arg.Any<GetAgentInfoRequest>())
            .Returns(CreateAgentInfoModelFixture(parentUsername).Create<AgentInfoModel>());

        _ = fakeAgentClient.GetAgentInfoByUsernameAsync(Arg.Any<GetAgentInfoByUsernameRequest>())
            .Returns(CreateAgentInfoModelFixture(parentUsername).Create<AgentInfoModel>());

        _ = fakeBrandServiceClient.GetBrandFullInfoAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(brandInfo);

        var sut = new SetAgentOneLayerCommandService(
            fakeAgentClient,
            fakeBrandServiceClient);

        var request = new SetAgentOneLayerCommand(agentId, parentUsername, 10, AgentType.Cash, "test");

        var response = await sut.GetAsync(request, default);

        Assert.That(response.Valid, Is.EqualTo(false));
        Assert.That(response.Message, Is.EqualTo(Services.BackOffice.Application.Resources.Agent.ParentAgentNotExists));

        _ = fakeAgentClient.ReceivedWithAnyArgs(0).SetOneLayerAgentInfoAsync(null!);
    }

    [Test]
    public async Task Can_Not_Set_One_Layer_Agent_When_Dividend_Percentage_Greater_Then_Parent()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeHttpContextMock = Substitute.For<IHttpContextAccessor>();
        var httpContextMock = Substitute.For<HttpContext>();

        _ = httpContextMock.User.Returns(GetUserClaimsPrincipal());

        fakeHttpContextMock.HttpContext = httpContextMock;

        var agentId = Guid.NewGuid();
        var username = "username";
        var parentUsername = "parentUsername";

        var brandInfo = CreateBrandFullInfoFixture(AgentDividendType.OneLayer).Create<BrandFullInfo>();

        _ = fakeAgentClient.GetAgentInfoAsync(Arg.Any<GetAgentInfoRequest>())
            .Returns(CreateAgentInfoModelFixture(username).Create<AgentInfoModel>());

        _ = fakeAgentClient.GetAgentInfoByUsernameAsync(Arg.Any<GetAgentInfoByUsernameRequest>())
            .Returns(CreateAgentInfoModelFixture(parentUsername).Create<AgentInfoModel>());

        _ = fakeBrandServiceClient.GetBrandFullInfoAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(brandInfo);

        var sut = new SetAgentOneLayerCommandService(
            fakeAgentClient,
            fakeBrandServiceClient);

        var request = new SetAgentOneLayerCommand(agentId, parentUsername, 99, AgentType.Cash, "test");

        var response = await sut.GetAsync(request, default);

        Assert.That(response.Valid, Is.EqualTo(false));
        Assert.That(response.Message, Is.EqualTo(Services.BackOffice.Application.Resources.Agent.DividendPercentageMustLessThanOrEqualToUpperLayer));

        _ = fakeAgentClient.ReceivedWithAnyArgs(0).SetOneLayerAgentInfoAsync(null!);
    }

    [Test]
    public void Update_Multiple_Layer_Agent()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeHttpContextMock = Substitute.For<IHttpContextAccessor>();
        var httpContextMock = Substitute.For<HttpContext>();

        _ = httpContextMock.User.Returns(GetUserClaimsPrincipal());

        fakeHttpContextMock.HttpContext = httpContextMock;

        var agentId = Guid.NewGuid();
        var username = "username";
        var parentUsername = "parentUsername";

        _ = fakeAgentClient.GetAgentInfoAsync(Arg.Any<GetAgentInfoRequest>())
            .Returns(CreateAgentInfoModelFixture(username).Create<AgentInfoModel>());

        _ = fakeAgentClient.GetAgentInfoByUsernameAsync(Arg.Any<GetAgentInfoByUsernameRequest>())
            .Returns(CreateAgentInfoModelFixture(parentUsername).Create<AgentInfoModel>());

        _ = fakeBrandServiceClient.GetBrandFullInfoAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(CreateBrandFullInfoFixture().Create<BrandFullInfo>());

        var sut = new SetAgentMultipleLayerCommandService(
            fakeAgentClient,
            fakeBrandServiceClient);

        var request = new SetAgentMultipleLayerCommand(agentId, parentUsername, [], AgentType.Cash, "test");

        _ = sut.GetAsync(request, default);

        _ = fakeAgentClient.ReceivedWithAnyArgs(1).SetMultipleLayerAgentInfoAsync(null!);
    }

    [Test]
    public void Set_Multiple_Layer_Agent()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeHttpContextMock = Substitute.For<IHttpContextAccessor>();
        var httpContextMock = Substitute.For<HttpContext>();

        _ = httpContextMock.User.Returns(GetUserClaimsPrincipal());

        fakeHttpContextMock.HttpContext = httpContextMock;

        var agentId = Guid.NewGuid();
        var username = "username";
        var parentUsername = "parentUsername";

        var brandInfo = CreateBrandFullInfoFixture(AgentDividendType.OneLayer).Create<BrandFullInfo>();

        _ = fakeAgentClient.GetAgentInfoAsync(Arg.Any<GetAgentInfoRequest>())
            .Returns(CreateAgentInfoModelFixture(username).Create<AgentInfoModel>());

        _ = fakeAgentClient.GetAgentInfoByUsernameAsync(Arg.Any<GetAgentInfoByUsernameRequest>())
            .Returns(CreateAgentInfoModelFixture(parentUsername).Create<AgentInfoModel>());

        _ = fakeBrandServiceClient.GetBrandFullInfoAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(brandInfo);

        var sut = new SetAgentMultipleLayerCommandService(
            fakeAgentClient,
            fakeBrandServiceClient);

        var request = new SetAgentMultipleLayerCommand(agentId, parentUsername, [], AgentType.Cash, "test");

        _ = sut.GetAsync(request, default);

        _ = fakeAgentClient.ReceivedWithAnyArgs(1).SetMultipleLayerAgentInfoAsync(null!);
    }

    [Test]
    public async Task Can_Not_Set_Multiple_Layer_Agent_When_Agent_Is_Null()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeHttpContextMock = Substitute.For<IHttpContextAccessor>();
        var httpContextMock = Substitute.For<HttpContext>();

        _ = httpContextMock.User.Returns(GetUserClaimsPrincipal());

        fakeHttpContextMock.HttpContext = httpContextMock;

        var agentId = Guid.NewGuid();
        var parentUsername = "parentUsername";

        _ = fakeAgentClient.GetAgentInfoAsync(Arg.Any<GetAgentInfoRequest>())
            .Returns((AgentInfoModel?)null);

        var sut = new SetAgentMultipleLayerCommandService(
            fakeAgentClient,
            fakeBrandServiceClient);

        var request = new SetAgentMultipleLayerCommand(agentId, parentUsername, [], AgentType.Cash, "test");

        var response = await sut.GetAsync(request, default);

        Assert.That(response.Valid, Is.EqualTo(false));
        Assert.That(response.Message, Is.EqualTo(Services.BackOffice.Application.Resources.Agent.NotFound));

        _ = fakeAgentClient.ReceivedWithAnyArgs(0).SetMultipleLayerAgentInfoAsync(null!);
    }

    [Test]
    public async Task Can_Not_Set_Multiple_Layer_Agent_When_Username_Same_With_Parent_Username()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeHttpContextMock = Substitute.For<IHttpContextAccessor>();
        var httpContextMock = Substitute.For<HttpContext>();

        _ = httpContextMock.User.Returns(GetUserClaimsPrincipal());

        fakeHttpContextMock.HttpContext = httpContextMock;

        var agentId = Guid.NewGuid();
        var parentUsername = "parentUsername";

        var brandInfo = CreateBrandFullInfoFixture(AgentDividendType.OneLayer).Create<BrandFullInfo>();

        _ = fakeAgentClient.GetAgentInfoAsync(Arg.Any<GetAgentInfoRequest>())
            .Returns(CreateAgentInfoModelFixture(parentUsername).Create<AgentInfoModel>());

        _ = fakeAgentClient.GetAgentInfoByUsernameAsync(Arg.Any<GetAgentInfoByUsernameRequest>())
            .Returns(CreateAgentInfoModelFixture(parentUsername).Create<AgentInfoModel>());

        _ = fakeBrandServiceClient.GetBrandFullInfoAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(brandInfo);

        var sut = new SetAgentMultipleLayerCommandService(
            fakeAgentClient,
            fakeBrandServiceClient);

        var request = new SetAgentMultipleLayerCommand(agentId, parentUsername, [], AgentType.Cash, "test");

        var response = await sut.GetAsync(request, default);

        Assert.That(response.Valid, Is.EqualTo(false));
        Assert.That(response.Message, Is.EqualTo(Services.BackOffice.Application.Resources.Agent.ParentAgentNotExists));

        _ = fakeAgentClient.ReceivedWithAnyArgs(0).SetMultipleLayerAgentInfoAsync(null!);
    }

    private static Fixture CreateAgentInfoModelFixture(string username, decimal dividendPercentage = 10)
    {
        var fixture = new Fixture();
        fixture.Customize<AgentInfoModel>(u => u
            .With(s => s.Username, username)
            .With(s => s.DividendPercentage, dividendPercentage));
        return fixture;
    }

    private static Fixture CreateBrandFullInfoFixture(AgentDividendType dividendType = AgentDividendType.OneLayer)
    {
        var fixture = new Fixture();
        fixture.Customize<BrandFullInfo>(u => u
            .With(s => s.AgentDividendType, dividendType));
        return fixture;
    }

    protected static ClaimsPrincipal GetUserClaimsPrincipal(
        string username = "admin.enabled",
        string roleName = "Admin",
        RoleType roleType = RoleType.Admin,
        string id = "e4ecfa98-b373-4123-87e0-cb9e883d0691")
        => new(
        [
            new ClaimsIdentity([
                new Claim(Consts.BackendOffice.UserInfo.UserIdKey, id),
                new Claim(Consts.BackendOffice.UserInfo.UsernameKey, username),
                new Claim(Consts.BackendOffice.UserInfo.RolenameKey, roleName),
                new Claim(Consts.BackendOffice.UserInfo.PortalKey, roleType.ToString())
            ])
        ]);

}