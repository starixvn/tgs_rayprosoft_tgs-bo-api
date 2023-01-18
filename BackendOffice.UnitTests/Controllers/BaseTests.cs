using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using TGS.Core.Models;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Tests.TestsCommon;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[Category(Named.Unit)]
public class BaseTests
{
    protected readonly Mock<HttpContext> HttpContextMock = new();
    protected string Username = "admin.enabled";

    public virtual void Setup()
    {
        Login();
    }

    protected static (int, int) GetPage(PageableRequest request)
    {
        var page = request.Page <= 0 ? 0 : request.Page - 1;
        var pageSize = request.Rows <= 0 ? 10 : request.Rows;
        return (page * pageSize, pageSize);
    }

    protected static ClaimsPrincipal GetUserClaimsPrincipal(
        string username = "admin.enabled",
        string rolename = "Admin",
        RoleType roleType = RoleType.Admin)
    {
        return new(new[]
        {
            new ClaimsIdentity(new[]
            {
                new Claim(Consts.BackendOffice.UserInfo.UsernameKey, username),
                new Claim(Consts.BackendOffice.UserInfo.RolenameKey, rolename),
                new Claim(Consts.BackendOffice.UserInfo.PortalKey, roleType.ToString())
            })
        });
    }

    private void Login(
                string username = "admin.enabled",
        string rolename = "Admin",
        RoleType roleType = RoleType.Admin)
    {
        _ = HttpContextMock.Setup(p => p.User)
            .Returns(GetUserClaimsPrincipal(username, rolename, roleType));
    }
}
