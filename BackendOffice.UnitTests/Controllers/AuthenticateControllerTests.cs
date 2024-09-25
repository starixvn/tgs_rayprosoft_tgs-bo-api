using System.Linq.Expressions;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using TGS.Apis.BackendOffice.Controllers;
using TGS.Apis.BackendOffice.Controllers.Messages.Users.Authentication;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Json;
using Tgs.Core.Infrastructures.Security;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class AuthenticateControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Can_Authenticate_User()
    {
        var antiForgeryMock = Substitute.For<IAntiforgery>();

        var authenticationServiceMock = Substitute.For<IAuthenticationService>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var passwordHandlerMock = Substitute.For<IPasswordHandler>();

        var serviceProviderMock = Substitute.For<IServiceProvider>();

        var userCommandsMock = Substitute.For<ITgsCommands<User>>();

        var userQueriesMock = Substitute.For<ITgsQueries<User>>();

        _ = serviceProviderMock
            .GetService(typeof(IAuthenticationService))
            .Returns(authenticationServiceMock);

        var controller =
            new AuthenticateController(passwordHandlerMock, antiForgeryMock, userQueriesMock, userCommandsMock,
                jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = new DefaultHttpContext { RequestServices = serviceProviderMock } }
            };

        var request = new AuthenticateUserRequest
        {
            Username = Generator.RandomString(6),
            Password = Generator.RandomString(6)
        };

        var user = CreateUserFixture().Create<User>();

        _ = userQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(),
                Arg.Any<Func<IQueryable<User>, IQueryable<User>>>())
            .Returns(Task.FromResult(user));

        _ = passwordHandlerMock.Verify(Arg.Any<byte[]>(), Arg.Any<byte[]>(), Arg.Any<string>())
            .Returns(true);

        _ = antiForgeryMock.GetAndStoreTokens(Arg.Any<HttpContext>())
            .Returns(new Fixture().Create<AntiforgeryTokenSet>());

        SetupHttpContext();

        var response = await controller.AuthenticateAsync(request);
        _ = response.Valid.Should().BeTrue();
        _ = response.UserInfo.Should().NotBeNull();
        await userCommandsMock.Received(1).UpdateAsync(Arg.Is<User>(u => u.Username == user.Username));
    }

    [Test]
    public async Task Can_Logout()
    {
        var antiForgeryMock = Substitute.For<IAntiforgery>();

        var authenticationServiceMock = Substitute.For<IAuthenticationService>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var passwordHandlerMock = Substitute.For<IPasswordHandler>();

        var serviceProviderMock = Substitute.For<IServiceProvider>();

        var userCommandsMock = Substitute.For<ITgsCommands<User>>();

        var userQueriesMock = Substitute.For<ITgsQueries<User>>();

        _ = serviceProviderMock
            .GetService(typeof(IAuthenticationService))
            .Returns(authenticationServiceMock);

        var controller =
            new AuthenticateController(passwordHandlerMock, antiForgeryMock, userQueriesMock, userCommandsMock,
                jsonSerializationProviderMock)
            {
                ControllerContext = { HttpContext = new DefaultHttpContext { RequestServices = serviceProviderMock } }
            };

        await controller.LogoutAsync();

        await authenticationServiceMock.SignOutAsync(Arg.Any<HttpContext>(), Arg.Any<string>(),
            Arg.Any<AuthenticationProperties>());
    }

    [Test]
    public async Task Cannot_Authenticate_User_Not_Found()
    {
        var antiForgeryMock = Substitute.For<IAntiforgery>();

        var authenticationServiceMock = Substitute.For<IAuthenticationService>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var passwordHandlerMock = Substitute.For<IPasswordHandler>();

        var serviceProviderMock = Substitute.For<IServiceProvider>();

        var userCommandsMock = Substitute.For<ITgsCommands<User>>();

        var userQueriesMock = Substitute.For<ITgsQueries<User>>();

        var controller = new AuthenticateController(passwordHandlerMock, antiForgeryMock, userQueriesMock,
            userCommandsMock, jsonSerializationProviderMock);

        var request = new AuthenticateUserRequest
        {
            Username = Generator.RandomString(6),
            Password = Generator.RandomString(6)
        };

        _ = userQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(),
                Arg.Any<Func<IQueryable<User>, IQueryable<User>>>())
            .Returns(Task.FromResult<User>(default!));

        var response = await controller.AuthenticateAsync(request);
        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.User.NotFound);
    }

    [Test]
    public async Task Cannot_Authenticate_User_With_Password_Not_Matched()
    {
        var antiForgeryMock = Substitute.For<IAntiforgery>();

        var authenticationServiceMock = Substitute.For<IAuthenticationService>();

        var jsonSerializationProviderMock = Substitute.For<IJsonSerializationProvider>();

        var passwordHandlerMock = Substitute.For<IPasswordHandler>();

        var serviceProviderMock = Substitute.For<IServiceProvider>();

        var userCommandsMock = Substitute.For<ITgsCommands<User>>();

        var userQueriesMock = Substitute.For<ITgsQueries<User>>();

        var controller = new AuthenticateController(passwordHandlerMock, antiForgeryMock, userQueriesMock,
            userCommandsMock, jsonSerializationProviderMock);

        var request = new AuthenticateUserRequest
        {
            Username = Generator.RandomString(6),
            Password = Generator.RandomString(6)
        };

        var user = CreateUserFixture().Create<User>();

        _ = userQueriesMock.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(),
                Arg.Any<Func<IQueryable<User>, IQueryable<User>>>())
            .Returns(Task.FromResult(user));

        _ = passwordHandlerMock.Verify(Arg.Any<byte[]>(), Arg.Any<byte[]>(), Arg.Any<string>())
            .Returns(false);

        var response = await controller.AuthenticateAsync(request);
        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.User.InvalidPassword);
    }

    private static Fixture CreateUserFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<User>(u => u
            .With(s => s.Role,
                new Role { Name = Generator.RandomString(5) })
            .With(p => p.Department,
                new Department { Name = Generator.RandomString(5) }));

        return fixture;
    }

    private static void SetupHttpContext()
    {
        var mockHttpContext = Substitute.For<HttpContext>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();

        _ = mockHttpContext.User
            .Returns(GetUserClaimsPrincipal());

        _ = mockHttpContext.RequestServices
            .Returns(mockServiceProvider);

        _ = mockHttpContext.Connection
            .Returns(Substitute.For<ConnectionInfo>());

        var httpResponse = Substitute.For<HttpResponse>();

        _ = httpResponse.Cookies
            .Returns(Substitute.For<IResponseCookies>());

        _ = mockHttpContext.Response
            .Returns(httpResponse);
    }
}