using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

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

        var state = new
        {
            executedContext.RouteData,
            RouteTemplate = executedContext.ActionDescriptor.AttributeRouteInfo?.Template,
            executedContext.HttpContext.Request.Method,
            UserName = executedContext.HttpContext.User.Identity.Name
        };

        _logger.Log(
            LogLevel.Information,
            new EventId(1000),
            state,
            null,
            (_, _) => "Record access log.");
    }
}
