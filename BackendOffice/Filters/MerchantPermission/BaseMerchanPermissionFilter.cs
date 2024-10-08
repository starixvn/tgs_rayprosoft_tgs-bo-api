using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using TGS.Core.Models;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Filters;

public class BaseMerchanPermissionFilter : IAsyncActionFilter
{
    private readonly ILogger<BaseMerchanPermissionFilter> logger;
    private readonly IMerchantRepository merchantRepository;
    private readonly string checkName;

    public BaseMerchanPermissionFilter(
        ILogger<BaseMerchanPermissionFilter> logger,
        IMerchantRepository merchantRepository,
        string checkName)
    {
        this.logger = logger;
        this.merchantRepository = merchantRepository;
        this.checkName = checkName;
    }

    public virtual async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (await HasPermissionAsync(context))
        {
            await next();
        }
        else
        {
            var username = context.HttpContext.User.FindFirst(ClaimTypes.Name).Value;
            logger.LogWarning($"{username} is trying to request other merchant data: ${string.Join(",", context.RouteData.Values.Select(item => $"{item.Key}: {item.Value}"))}");
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
        }
    }

    protected virtual string GetValue(ActionExecutingContext context)
    {
        return string.Empty;
    }

    protected RoleType GetUserRoleType(ActionExecutingContext context)
    {
        var roleTypeString = context.HttpContext.User.FindFirst(Consts.BackendOffice.UserInfo.RoleTypeKey)!.Value;
        return (RoleType)Enum.Parse(typeof(RoleType), roleTypeString);
    }

    protected string GetUserResellerId(ActionExecutingContext context)
    {
        return context.HttpContext.User.FindFirst(Consts.BackendOffice.UserInfo.ResellerKey).Value;
    }

    protected string GetUserLicenseeId(ActionExecutingContext context)
    {
        return context.HttpContext.User.FindFirst(Consts.BackendOffice.UserInfo.LicenseeKey).Value;
    }

    protected string GetUserBrandId(ActionExecutingContext context)
    {
        return context.HttpContext.User.FindFirst(Consts.BackendOffice.UserInfo.BrandKey).Value;
    }

    private async Task<bool> HasPermissionAsync(ActionExecutingContext context)
    {
        var value = GetValue(context);
        if (string.IsNullOrEmpty(value)) { return false; }
        switch (checkName)
        {
            case "reseller":
                return IsAdmin(context) || (GetUserResellerId(context)).Equals(GetValue(context));

            case "licensee":
                return await IsValidLicenseeIdAsync(context);

            case "brand":
                return await IsValidBrandIdAsync(context);

            default:
                return false;
        }
    }

    private async Task<bool> IsValidBrandIdAsync(ActionExecutingContext context)
    {
        var roleType = GetUserRoleType(context);
        var brandIdString = GetValue(context);
        var brandId = Guid.Parse(brandIdString);
        switch (roleType)
        {
            case RoleType.Admin:
                return true;

            case RoleType.Reseller:
                {
                    var resellerId = GetUserResellerId(context);
                    if (string.IsNullOrEmpty(resellerId))
                    {
                        return false;
                    }
                    return await merchantRepository.ExistBrandInResellersAsync(brandId, Guid.Parse(resellerId));
                }
            case RoleType.Licensee:
                {
                    var licenseeId = GetUserLicenseeId(context);
                    if (string.IsNullOrEmpty(licenseeId))
                    {
                        return false;
                    }
                    return await merchantRepository.ExistBrandInLicenseesAsync(brandId, Guid.Parse(licenseeId));
                }
            default:
                return Guid.Parse(GetUserBrandId(context)).Equals(brandId);
        }
    }

    private async Task<bool> IsValidLicenseeIdAsync(ActionExecutingContext context)
    {
        var roleType = GetUserRoleType(context);
        var licenseeIdString = GetValue(context);
        var licenseeId = Guid.Parse(licenseeIdString);
        switch (roleType)
        {
            case RoleType.Admin:
                return true;

            case RoleType.Reseller:
                {
                    var resellerId = GetUserResellerId(context);
                    if (string.IsNullOrEmpty(resellerId))
                    {
                        return false;
                    }
                    return await merchantRepository.ExistLicenseeInResellersAsync(licenseeId, Guid.Parse(resellerId));
                }
            default:
                return Guid.Parse(GetUserLicenseeId(context)).Equals(licenseeId);
        }
    }

    private bool IsAdmin(ActionExecutingContext context)
    {
        var roleType = GetUserRoleType(context);
        return roleType == RoleType.Admin;
    }
}