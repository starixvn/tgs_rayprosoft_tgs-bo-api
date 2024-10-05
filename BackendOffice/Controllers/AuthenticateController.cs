using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing.Charts;
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

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api")]
public class AuthenticateController(
    IPasswordHandler passwordHandler,
    IAntiforgery antiForgery,
    ITgsQueries<User> userQueries,
    ITgsCommands<User> userCommands,
    IJsonSerializationProvider jsonSerializationProvider) : BaseContextController
{
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [HttpPost("auth")]
    [Description("Authenticate user login")]
    public async Task<AuthenticateUserResponse> AuthenticateAsync([FromBody] AuthenticateUserRequest request)
    {
        User user;
        if (!string.IsNullOrEmpty(request.Token))
        {
            user = await userQueries.GetSingleAsync(p => p.ReferenceId == request.ReferenceId,
            u => u.Include(p => p.Role));
            if (!CheckSsoUser(user, request))
            {
                user = null;
            }
        }
        else
        {
            user = await userQueries.GetSingleAsync(p => p.Username == request.Username,
                u => u.Include(p => p.Role));
        }

        if (user == null || user.Status != UserStatus.Enabled)
        {
            return CreateInvalidResponse<AuthenticateUserResponse>(Resources.User.NotFound);
        }

        if (string.IsNullOrEmpty(request.Token) && !passwordHandler.Verify(user.Password, user.Salt, request.Password))
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

        await userCommands.UpdateAsync(user);
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
                Menus = jsonSerializationProvider.DeserializeFromString<Core.Models.Services.MenuSetting[]>(user.Role
                    .MenuSetting)
            }
        };
    }

    [HttpPost]
    [Route("logout")]
    public Task LogoutAsync() => HttpContext.SignOutAsync();

    private void SetCsrfToken()
    {
        var tokens = antiForgery.GetAndStoreTokens(HttpContext);

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

    private bool CheckSsoUser(User user, AuthenticateUserRequest request)
    {
        var token = "";
        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        using (var hmacsha256 = new HMACSHA256(user.Salt))
        {
            var hashMessage = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes($"{request.ReferenceId}-{request.Timestamp}"));
            token = Convert.ToHexString(hashMessage);
        }

        return token == request.Token && currentTimestamp - long.Parse(request.Timestamp) < 120;
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
            new(Consts.BackendOffice.UserInfo.PortalKey, user.Role.RoleType.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
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