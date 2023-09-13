using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TGS.Apis.BackendOffice.Middlewares;
using TGS.Core.Infrastructures.Json;
using TGS.Core.Infrastructures.WebApiServices;
using TGS.Core.Models.Messages;
using Tgs.Logging;

namespace TGS.Tests.BackendOffice.UnitTests.Middlewares;

[TestFixture]
[Parallelizable]
public class ExceptionMiddlewareTest
{
    private readonly string _requestString = "Hello World Request!!";

    private readonly string _responseString = "Hello World Response!!";

    ///TODO: fix  cannot pass   using var sr = new StreamReader(context.Response.Body);
    [Test]
    public async Task Can_Invoke()
    {
        var envMock = Substitute.For<IWebHostEnvironment>();

        var jsonMock = Substitute.For<IJsonSerializationProvider>();

        var requestAnalyzerMock = Substitute.For<IRequestAnalyzer>();

        var tgsLogMock = Substitute.For<ITgsLog>();

        Task RequestDelegate(HttpContext innerContext) => Task.FromResult(0);

        IMiddleware target = new ExceptionMiddleware(
            requestAnalyzerMock,
            tgsLogMock,
            jsonMock,
            envMock,
            new NullLogger<ExceptionMiddleware>());

        var httpContextMock = SetupHttpContext(jsonMock);

        var action = () => target.InvokeAsync(httpContextMock, RequestDelegate);

        _ = await action.Should().NotThrowAsync();

        tgsLogMock.Received(1).LogAdmin(Arg.Any<AdminLogEvent>());
    }

    [Test]
    public async Task Can_Invoke_With_Exception()
    {
        var envMock = Substitute.For<IWebHostEnvironment>();

        var jsonMock = Substitute.For<IJsonSerializationProvider>();

        var requestAnalyzerMock = Substitute.For<IRequestAnalyzer>();

        var tgsLogMock = Substitute.For<ITgsLog>();

        Task RequestDelegate(HttpContext innerContext) => Task.FromResult(0);

        IMiddleware target = new ExceptionMiddleware(
            requestAnalyzerMock,
            tgsLogMock,
            jsonMock,
            envMock,
            new NullLogger<ExceptionMiddleware>());

        var httpContextMock = SetupHttpContext(jsonMock, false);

        var action = () => target.InvokeAsync(httpContextMock, RequestDelegate);

        _ = await action.Should().NotThrowAsync();

        tgsLogMock.Received(1).LogAdmin(Arg.Is<AdminLogEvent>(l =>
            l.Exception != string.Empty));
    }

    private HttpContext SetupHttpContext(IJsonSerializationProvider jsonSerializationProvider,
        bool hasNoException = true)
    {
        //https://andrewcampey.com/2017/12/07/c-memory-stream-is-not-expandable/
        // This CANNOT be extended   for error : memory stream is not expandable
        var streamRequest = new MemoryStream(Encoding.UTF8.GetBytes(_requestString));
        var streamResponse = new MemoryStream(); // // This CAN be extended
        var responseStringByteArray = Encoding.UTF8.GetBytes(_responseString);
        streamResponse.Write(responseStringByteArray, 0, responseStringByteArray.Length);

        var mockHttpContext = Substitute.For<HttpContext>();

        _ = mockHttpContext.Connection
            .Returns(Substitute.For<ConnectionInfo>());

        var mockHttpResponse = Substitute.For<HttpResponse>();
        _ = mockHttpResponse.Cookies
            .Returns(Substitute.For<IResponseCookies>());
        _ = mockHttpResponse.Body
            .Returns(streamResponse);

        var buff = new byte[1024];

        _ = mockHttpResponse
            .Body.WriteAsync(buff,
                0,
                1024,
                default);

        _ = hasNoException
            ? mockHttpResponse.Headers
                .Returns(new HeaderDictionary())
            : mockHttpResponse.Headers
                .Returns((IHeaderDictionary)null!);

        _ = mockHttpContext.Response
            .Returns(mockHttpResponse);

        var mockHttpRequest = Substitute.For<HttpRequest>();
        _ = mockHttpRequest.Body.Returns(streamRequest);
        _ = mockHttpRequest.Path.Returns((PathString)"/api/path");
        _ = mockHttpRequest.Headers
            .Returns(new HeaderDictionary());

        _ = mockHttpContext.Request
            .Returns(mockHttpRequest);

        var fc = new FeatureCollection();
        var endPoint = new Endpoint(c => Task.CompletedTask, new(), "for unit test");
        var ehf = new ExceptionHandlerFeature();
        fc.Set(endPoint);
        fc.Set(ehf);

        _ = mockHttpContext.Features
            .Returns(fc);

        _ = jsonSerializationProvider.SerializeToString(Arg.Any<ErrorDetails>(), Arg.Any<JsonSerializerOptions>())
            .Returns("test");
        return mockHttpContext;
    }
}