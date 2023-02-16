#nullable enable

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Core.Models;

namespace TGS.Apis.BackendOffice.Filters;

public class AccessLogFilter : IAsyncActionFilter
{
    private readonly ILogger<AccessLogFilter> _logger;

    public AccessLogFilter(ILogger<AccessLogFilter> logger)
    {
        _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executedContext = await next();

        var actionAttr = executedContext.ActionDescriptor.EndpointMetadata.OfType<TgsActionAttribute>().FirstOrDefault();

        if (actionAttr is null)
            return;

        var state = new
        {
            executedContext.RouteData,
            RouteTemplate = executedContext.ActionDescriptor.AttributeRouteInfo?.Template,
            executedContext.HttpContext.Request.Method,
            UserName = executedContext.HttpContext.User.Identity?.Name,
            UserId = executedContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            BrandId = executedContext.HttpContext.User.FindFirst(Consts.BackendOffice.UserInfo.BrandKey)?.Value,
            TgsAction = actionAttr.Name,
            Claims = executedContext.HttpContext.User.Claims.Select(c => new { Key = c.ValueType, Value = c.Value })
        };

        _logger.Log(
            LogLevel.Information,
            new EventId(1000),
            state,
            null,
            (_, _) => "Record access log.");
    }
}
