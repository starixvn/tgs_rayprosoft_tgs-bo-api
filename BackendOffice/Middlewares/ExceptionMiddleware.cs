using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Infrastructures.Json;
using TGS.Core.Infrastructures.Utility;
using TGS.Core.Infrastructures.WebApiServices;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Middlewares;

public class ExceptionMiddleware : IMiddleware
{
    private const int ResMessageSizeLimit = 8000;
    private readonly IWebHostEnvironment _env;
    private readonly IJsonSerializationProvider _json;
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
    private readonly IRequestAnalyzer _requestAnalyzer;
    private readonly ILogger<ExceptionMiddleware> _systemLogger;

    public ExceptionMiddleware(
        IRequestAnalyzer requestAnalyzer,
        IJsonSerializationProvider json,
        IWebHostEnvironment env,
        ILogger<ExceptionMiddleware> systemLogger)
    {
        _requestAnalyzer = requestAnalyzer;
        _json = json;
        _env = env;
        _recyclableMemoryStreamManager = new();
        _systemLogger = systemLogger ?? throw new ArgumentNullException(nameof(systemLogger));
        _recyclableMemoryStreamManager = new();
    }

    async Task IMiddleware.InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        var defaultBypassRouting = new[]
        {
            "/health",
            "/metrics",
            "/favicon.ico",
            "/swagger"
        };

        var attribute = httpContext.Features.Get<IEndpointFeature>()?.Endpoint?.Metadata
            .GetMetadata<ByPassAdminLogAttribute>();

        try
        {
            var reqMessage = await GetRequestBodyAsync(httpContext);

            var (resMessage, ms) = await GetResponseMessageAsync(httpContext, next);

            var descriptionAttribute = httpContext.GetEndpoint()?.Metadata.GetMetadata<DescriptionAttribute>();

            if (!defaultBypassRouting.Any(httpContext.Request.Path.Value.Contains)
                && attribute == null)
            {
                var adminLogEvent = new
                {
                    IpAddress = _requestAnalyzer.GetIp(httpContext)?.NoLongerThan(256),
                    Path = httpContext.Request.Path.Value,
                    Method = httpContext.Request.Method.NoLongerThan(8),
                    Host = httpContext.Request.Host.Value,
                    ServerName = Environment.MachineName.NoLongerThan(128),
                    RequestHeaders = httpContext.Request.Headers.ToDetailString(),
                    RequestContent = reqMessage,
                    ResponseHeaders = httpContext.Response.Headers.ToDetailString(),
                    RecordedOn = DateTimeOffset.Now,
                    StackTrace = string.Empty,
                    Exception = string.Empty,
                    descriptionAttribute?.Description,
                    ResponseContent = resMessage?.Length > ResMessageSizeLimit
                    ? CutOffResponseContent(resMessage)
                    : resMessage,
                    ResponseCode = (short)httpContext.Response.StatusCode,
                    ExecutionTimeMs = ms
                };

                _systemLogger.LogInformation("{logEvent}", adminLogEvent);
            }
        }
        catch (Exception ex)
        {
            _systemLogger.LogError(ex, "Error!");

            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            httpContext.Response.ContentType = "application/json";

            var message = new ErrorDetails
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "Internal Server Error."
            };

            if (_env.IsDevelopment())
            {
                message.Message = ex.Message;
            }

            var result = _json.SerializeToString(message);
            await httpContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(result));
        }
    }

    private static string CutOffResponseContent(string responseContent) =>
        $"{responseContent.NoLongerThan(ResMessageSizeLimit)} ... Response content is too long. Length: {responseContent.Length}";

    private async Task<string> GetRequestBodyAsync(HttpContext context)
    {
        context.Request.EnableBuffering();

        if ((context.Request.ContentLength ?? 0L) == 0)
            return string.Empty;

        await using var requestStream = _recyclableMemoryStreamManager.GetStream();
        await context.Request.Body.CopyToAsync(requestStream);
        _ = requestStream.Seek(0, SeekOrigin.Begin);
        var sr = new StreamReader(requestStream);
        var message = await sr.ReadToEndAsync();
        _ = context.Request.Body.Seek(0, SeekOrigin.Begin);
        return message;
    }

    private async Task<(string resMessage, int elapsedMilliseconds)> GetResponseMessageAsync(HttpContext context,
        RequestDelegate next)
    {
        var originalBodyStream = context.Response.Body;
        var responseStream = _recyclableMemoryStreamManager.GetStream();
        context.Response.Body = responseStream;

        try
        {
            var sw = Stopwatch.StartNew();
            await next(context);
            sw.Stop();
            var ms = (int)sw.ElapsedMilliseconds;
            _ = context.Response.Body.Seek(0, SeekOrigin.Begin);
            var message = await StreamToString.GetStringAsync(context.Response.Body);
            _ = context.Response.Body.Seek(0, SeekOrigin.Begin);
            await context.Response.Body.CopyToAsync(originalBodyStream);
            return (message, ms);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
            await responseStream.DisposeAsync();
        }
    }
}
