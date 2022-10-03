using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Models;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application;

public abstract class Handler<T, TOut> : IRequestHandler<T, TOut> where T : IRequest<TOut>
{
    private readonly Dictionary<string, string> _userInfoMap;

    protected string UserId => _userInfoMap[Consts.BackendOffice.UserInfo.UserIdKey];
    protected string Username => _userInfoMap[Consts.BackendOffice.UserInfo.UsernameKey];
    protected string RoleName => _userInfoMap[Consts.BackendOffice.UserInfo.RolenameKey];

    protected Guid? ResellerId => GetIdByKey(Consts.BackendOffice.UserInfo.ResellerKey);

    protected Guid? LicenseeId => GetIdByKey(Consts.BackendOffice.UserInfo.LicenseeKey);
    protected Guid? BrandId => GetIdByKey(Consts.BackendOffice.UserInfo.BrandKey);

    protected Handler(IHttpContextAccessor accessor)
    {
        if (accessor.HttpContext != null)
        {
            _userInfoMap = accessor.HttpContext.User.Claims.ToDictionary(p => p.Type, p => p.Value);
        }
    }

    public abstract Task<TOut> Handle(T request, CancellationToken cancellationToken);

    protected static TResponse CreateValidResponse<TResponse>() where TResponse : MessageResponse, new() =>
        new()
        {
            Valid = true
        };

    protected static TResponse CreateInvalidResponse<TResponse>(string message)
        where TResponse : MessageResponse, new() =>
        new()
        {
            Valid = false,
            Message = message
        };

    protected static TResponse CreateInvalidResponse<TResponse>(string message, string[] errors)
        where TResponse : MessageResponse, new() =>
        new()
        {
            Valid = false,
            Message = message,
            Errors = errors
        };

    private Guid? GetIdByKey(string key) =>
        !_userInfoMap.TryGetValue(key, out var value) || value.IsNullOrEmpty() ||
        !Guid.TryParse(value, out var id)
            ? (Guid?)null
            : id;
}