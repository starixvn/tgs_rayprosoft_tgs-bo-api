using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models;
using TGS.Services.BackOffice.Application.Models;

namespace TGS.Services.BackOffice.Application.Middlewares;

public class PipelineBehavior<TIn, TOut> : IPipelineBehavior<TIn, TOut> where TIn : IRequest<TOut>
{
    private readonly HttpContext _httpContext;

    public PipelineBehavior(IHttpContextAccessor accessor)
    {
        _httpContext = accessor.HttpContext;
    }

    public async Task<TOut> Handle(
        TIn request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TOut> next)
    {
        var userInfoMap = _httpContext.User.Claims.ToDictionary(p => p.Type, p => p.Value);
        var property = request.GetType().GetProperty("EditorUsername");
        property?.SetValue(request, userInfoMap[Consts.BackendOffice.UserInfo.UsernameKey]);

        if (request is BaseRequest br)
        {
            br.EditorUsername = userInfoMap[Consts.BackendOffice.UserInfo.UsernameKey];
        }

        var result = await next();

        return result;
    }
}