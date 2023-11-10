using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using TGS.Core.Models;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Tests.TestsCommon;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[Category(Named.Unit)]
public class BaseTests
{
    protected readonly HttpContext HttpContextMock = Substitute.For<HttpContext>();
    protected string Username = "admin.enabled";

    public virtual void Setup() => Login();

    protected static (int, int) GetPage(PageableRequest request)
    {
        var page = request.Page <= 0 ? 0 : request.Page - 1;
        var pageSize = request.Rows <= 0 ? 10 : request.Rows;
        return (page * pageSize, pageSize);
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

    public void Login(
        string username = "admin.enabled",
        string roleName = "Admin",
        RoleType roleType = RoleType.Admin)
        => _ = HttpContextMock.User
            .Returns(GetUserClaimsPrincipal(username, roleName, roleType));
}