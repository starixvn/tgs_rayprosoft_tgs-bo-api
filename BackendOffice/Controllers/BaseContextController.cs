using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Models;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers;

public class BaseContextController : ControllerBase
{
    protected Guid? BrandId => GetUserMap().GetIdByKey(Consts.BackendOffice.UserInfo.BrandKey);

    protected Guid? LicenseeId => GetUserMap().GetIdByKey(Consts.BackendOffice.UserInfo.LicenseeKey);

    protected Guid? ResellerId => GetUserMap().GetIdByKey(Consts.BackendOffice.UserInfo.ResellerKey);

    protected string RoleName => GetUserMap()[Consts.BackendOffice.UserInfo.RolenameKey];

    protected string UserId => GetUserMap()[Consts.BackendOffice.UserInfo.UserIdKey];

    protected string Username => GetUserMap()[Consts.BackendOffice.UserInfo.UsernameKey];

    protected bool IsAdmin =>
        GetUserMap().GetTypeByKey<RoleType>(Consts.BackendOffice.UserInfo.RoleTypeKey) == RoleType.Admin;

    protected RoleType? UserRoleType =>
        GetUserMap().GetTypeByKey<RoleType>(Consts.BackendOffice.UserInfo.RoleTypeKey);

    protected static TResponse CreateInvalidResponse<TResponse>(string message)
        where TResponse : MessageResponse, new() =>
        new()
        {
            Valid = false,
            Message = message
        };

    protected static TResponse CreateValidResponse<TResponse>() where TResponse : MessageResponse, new() =>
        new()
        {
            Valid = true
        };

    private Dictionary<string, string> GetUserMap()
        => HttpContext.User.Claims.ToDictionary(p => p.Type, p => p.Value);
}
