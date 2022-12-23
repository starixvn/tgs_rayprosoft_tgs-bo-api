using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tgs.Core.Infrastructures.Security;
using TGS.Apis.BackendOffice.Controllers.Messages.Users.Authentication;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Json;
using TGS.Core.Models;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Services;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api")]
public class AuthenticateController : BaseContextController
{
    private readonly IAntiforgery _antiForgery;
    private readonly IJsonSerializationProvider _jsonSerializationProvider;
    private readonly IPasswordHandler _passwordHandler;
    private readonly ITgsCommands<User> _userCommands;
    private readonly ITgsQueries<User> _userQueries;

    public AuthenticateController(
        IPasswordHandler passwordHandler,
        IAntiforgery antiForgery,
        ITgsQueries<User> userQueries,
        ITgsCommands<User> userCommands,
        IJsonSerializationProvider jsonSerializationProvider)
    {
        _passwordHandler = passwordHandler;
        _antiForgery = antiForgery;
        _userQueries = userQueries;
        _userCommands = userCommands;
        _jsonSerializationProvider = jsonSerializationProvider;
    }

    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [HttpPost("auth")]
    [Description("Authenticate user login")]
    public async Task<AuthenticateUserResponse> AuthenticateAsync([FromBody] AuthenticateUserRequest request)
    {
        var user = await _userQueries.GetSingleAsync(p => p.Username == request.Username,
            u => u.Include(p => p.Role));

        if (user == null)
        {
            return CreateInvalidResponse<AuthenticateUserResponse>(Resources.User.NotFound);
        }

        if (!_passwordHandler.Verify(user.Password, user.Salt, request.Password))
        {
            return CreateInvalidResponse<AuthenticateUserResponse>(Resources.User.InvalidPassword);
        }

        if (HttpContext.Connection.RemoteIpAddress != null)
        {
            user.LastLoginIp = HttpContext.Connection.RemoteIpAddress.ToString();
        }

        if (user.Status == UserStatus.Disabled)
        {
            return CreateInvalidResponse<AuthenticateUserResponse>(Resources.User.StatusDisabled);
        }

        user.LastLoginTime = DateTimeOffset.Now;

        await _userCommands.UpdateAsync(user);
        await SignInAsync(user);

        return new()
        {
            Valid = true,
            UserInfo = new()
            {
                Role = user.Role.Name,
                RoleId = user.Role.Id,
                IsRoot = user.Role.IsRoot,
                ResellerId = user.Role.ResellerId,
                LicenseeId = user.Role.LicenseeId,
                BrandId = user.Role.BrandId,
                RoleType = user.Role.RoleType,
                Username = user.Username,
                UserId = user.Id,
                TimeZone = user.TimeZone,
                Menus = _jsonSerializationProvider.DeserializeFromString<MenuSetting[]>(user.Role
                    .MenuSetting)
            }
        };
    }

    [HttpPost]
    [Route("logout")]
    public Task LogoutAsync() => HttpContext.SignOutAsync();

    private void SetCsrfToken()
    {
        var tokens = _antiForgery.GetAndStoreTokens(HttpContext);

        HttpContext.Response.Cookies.Append(
            Consts.BackendOffice.CsrfTokenName,
            tokens.RequestToken ?? string.Empty,
            new()
            {
                HttpOnly = false,
                Path = "/",
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            }
        );
    }

    private Task SignInAsync(User user)
    {
        SetCsrfToken();

        var claims = new List<Claim>
        {
            new(Consts.BackendOffice.UserInfo.UserIdKey, user.Id.ToString()),
            new(Consts.BackendOffice.UserInfo.UsernameKey, user.Username),
            new(Consts.BackendOffice.UserInfo.RolenameKey, user.Role.Name),
            new(Consts.BackendOffice.UserInfo.RoleTypeKey, user.Role.RoleType.ToString()),
            new(Consts.BackendOffice.UserInfo.ResellerKey, user.Role.ResellerId.ToString()),
            new(Consts.BackendOffice.UserInfo.LicenseeKey, user.Role.LicenseeId.ToString()),
            new(Consts.BackendOffice.UserInfo.BrandKey, user.Role.BrandId.ToString()),
            new(Consts.BackendOffice.UserInfo.PortalKey, user.Role.RoleType.ToString())
        };

        var authProperties = new AuthenticationProperties
        {
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1),
            IsPersistent = true
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        return HttpContext.SignInAsync(new(claimsIdentity), authProperties);
    }
}
