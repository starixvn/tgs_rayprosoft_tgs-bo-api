using System.Linq.Expressions;
using System.Security.Claims;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Moq;
using Tgs.Core.Infrastructures.Security;
using TGS.Apis.BackendOffice.Controllers;
using TGS.Apis.BackendOffice.Controllers.Messages.Users.Authentication;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Json;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class AuthenticateControllerTests : BaseTests
{
    private readonly Mock<IAntiforgery> _antiForgeryMock = new();

    private readonly Mock<IAuthenticationService> _authenticationServiceMock = new();

    private readonly Mock<IJsonSerializationProvider> _jsonSerializationProviderMock =
        new();

    private readonly Mock<IPasswordHandler> _passwordHandlerMock = new();

    private readonly Mock<IServiceProvider> _serviceProviderMock = new();

    private readonly Mock<ITgsCommands<User>> _userCommandsMock = new();

    private readonly Mock<ITgsQueries<User>> _userQueriesMock = new();

    private AuthenticateController _controller = default!;

    [Test]
    public async Task Can_Authenticate_User()
    {
        var request = new AuthenticateUserRequest
        {
            Username = Generator.RandomString(6),
            Password = Generator.RandomString(6)
        };

        var user = CreateUserFixture().Create<User>();

        _ = _userQueriesMock.Setup(u => u.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IQueryable<User>>>()))
            .Returns(Task.FromResult(user));

        _ = _passwordHandlerMock.Setup(p => p.Verify(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<string>()))
            .Returns(true);

        _ = _antiForgeryMock.Setup(p => p.GetAndStoreTokens(It.IsAny<HttpContext>()))
            .Returns(new Fixture().Create<AntiforgeryTokenSet>());

        SetupHttpContext();

        var response = await _controller.AuthenticateAsync(request);
        _ = response.Valid.Should().BeTrue();
        _ = response.UserInfo.Should().NotBeNull();
        _userCommandsMock.Verify(p => p.UpdateAsync(It.Is<User>(u => u.Username == user.Username)));
    }

    [Test]
    public async Task Can_Logout()
    {
        //SetupHttpContext();
        await _controller.LogoutAsync();

        _authenticationServiceMock.Verify(p =>
            p.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()));
    }

    [Test]
    public async Task Cannot_Authenticate_User_Not_Found()
    {
        var request = new AuthenticateUserRequest
        {
            Username = Generator.RandomString(6),
            Password = Generator.RandomString(6)
        };

        _ = _userQueriesMock.Setup(u => u.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IQueryable<User>>>()))
            .Returns(Task.FromResult<User>(default!));

        var response = await _controller.AuthenticateAsync(request);
        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.User.NotFound);
    }

    [Test]
    public async Task Cannot_Authenticate_User_With_Password_Not_Matched()
    {
        var request = new AuthenticateUserRequest
        {
            Username = Generator.RandomString(6),
            Password = Generator.RandomString(6)
        };

        var user = CreateUserFixture().Create<User>();

        _ = _userQueriesMock.Setup(u => u.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IQueryable<User>>>()))
            .Returns(Task.FromResult(user));

        _ = _passwordHandlerMock.Setup(p => p.Verify(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<string>()))
            .Returns(false);

        var response = await _controller.AuthenticateAsync(request);
        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.User.InvalidPassword);
    }

    [SetUp]
    public override void Setup()
    {
        base.Setup();

        _controller = new AuthenticateController(
            _passwordHandlerMock.Object,
            _antiForgeryMock.Object,
            _userQueriesMock.Object,
            _userCommandsMock.Object,
            _jsonSerializationProviderMock.Object);

        //ref:https://stackoverflow.com/questions/47198341/how-to-unit-test-httpcontext-signinasync
        _ = _serviceProviderMock
            .Setup(_ => _.GetService(typeof(IAuthenticationService)))
            .Returns(_authenticationServiceMock.Object);

        _controller.ControllerContext.HttpContext = new DefaultHttpContext
        {
            RequestServices = _serviceProviderMock.Object
        };
    }

    private static Fixture CreateUserFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<User>(u => u
            .With(s => s.Role,
                new Role
                {
                    Name = Generator.RandomString(5)
                })
            .With(p => p.Department,
                new Department
                {
                    Name = Generator.RandomString(5)
                }));

        return fixture;
    }

    private void SetupHttpContext()
    {
        var mockHttpContext = new Mock<HttpContext>();

        _ = mockHttpContext.Setup(p => p.User)
            .Returns(GetUserClaimsPrincipal());

        _ = mockHttpContext.Setup(p => p.RequestServices)
            .Returns(SetupServiceProviderMock().Object);

        _ = mockHttpContext.Setup(p => p.Connection)
            .Returns(new Mock<ConnectionInfo>().Object);

        var httpResponse = new Mock<HttpResponse>();

        _ = httpResponse.Setup(p => p.Cookies)
            .Returns(new Mock<IResponseCookies>().Object);

        _ = mockHttpContext.Setup(p => p.Response)
            .Returns(httpResponse.Object);
    }

    private Mock<IServiceProvider> SetupServiceProviderMock()
    {
        _ = _authenticationServiceMock
            .Setup(_ => _.SignInAsync(It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties>()))
            .Returns(Task.FromResult(default(object)));

        _ = _authenticationServiceMock
            .Setup(_ => _.SignOutAsync(It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<AuthenticationProperties>()))
            .Returns(Task.FromResult(default(object)));

        var serviceProviderMock = new Mock<IServiceProvider>();

        _ = serviceProviderMock
            .Setup(_ => _.GetService(typeof(IAuthenticationService)))
            .Returns(_authenticationServiceMock.Object);

        return serviceProviderMock;
    }
}
