#nullable enable

using System.Security.Claims;
using TGS.Core.Models;

namespace TGS.Apis.BackendOffice.Controllers;

public static class IdentityExtensions
{
    public static string? GetUserName(this ClaimsPrincipal user)
        => user.FindFirstValue(Consts.BackendOffice.UserInfo.UsernameKey);
}
