using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Tgs.Logging;
using TGS.Apis.BackendOffice.Middlewares;
using TGS.Core.Infrastructures.Json;
using TGS.Core.Infrastructures.WebApiServices;
using TGS.Core.Models.Messages;

namespace TGS.Tests.BackendOffice.UnitTests.Middlewares;

[TestFixture]
[Parallelizable]
public class ExceptionMiddlewareTest
{
    private readonly Mock<IWebHostEnvironment> _envMock = new();

    private readonly Mock<IJsonSerializationProvider> _json = new();

    private readonly Mock<IRequestAnalyzer> _requestAnalyzerMock = new();

    private readonly string _requestString = "Hello World Request!!";

    private readonly string _responseString = "Hello World Response!!";

    private readonly Mock<ITgsLog> _tgsLogMock = new();

    private RequestDelegate _requestDelegate = default!;

    private IMiddleware _target = default!;

    ///TODO: fix  cannot pass   using var sr = new StreamReader(context.Response.Body);
    [Test]
    public async Task Can_Invoke()
    {
        var httpContextMock = SetupHttpContext();
        Func<Task> action = () => _target.InvokeAsync(httpContextMock.Object, _requestDelegate);

        _ = await action.Should().NotThrowAsync();

        _tgsLogMock.Verify(p => p.LogAdmin(It.IsAny<AdminLogEvent>()));
    }

    [Test]
    public async Task Can_Invoke_With_Exception()
    {
        var httpContextMock = SetupHttpContext(false);
        Func<Task> action = () => _target.InvokeAsync(httpContextMock.Object, _requestDelegate);

        _ = await action.Should().NotThrowAsync();

        _tgsLogMock.Verify(p => p.LogAdmin(It.Is<AdminLogEvent>(l =>
            l.Exception != string.Empty)));
    }

    [SetUp]
    public void Setup()
    {
        _requestDelegate = innerContext => Task.FromResult(0);
        _target = new ExceptionMiddleware(
            _requestAnalyzerMock.Object,
            _tgsLogMock.Object,
            _json.Object,
            _envMock.Object,
            new NullLogger<ExceptionMiddleware>());
    }

    private Mock<HttpContext> SetupHttpContext(bool hasNoException = true)
    {
        //https://andrewcampey.com/2017/12/07/c-memory-stream-is-not-expandable/
        // This CANNOT be extended   for error : memory stream is not expandable
        var streamRequest = new MemoryStream(Encoding.UTF8.GetBytes(_requestString));
        var streamResponse = new MemoryStream(); // // This CAN be extended
        var responseStringByteArray = Encoding.UTF8.GetBytes(_responseString);
        streamResponse.Write(responseStringByteArray, 0, responseStringByteArray.Length);

        var mockHttpContext = new Mock<HttpContext>();

        _ = mockHttpContext.Setup(p => p.Connection)
            .Returns(new Mock<ConnectionInfo>().Object);

        var mockHttpResponse = new Mock<HttpResponse>();
        _ = mockHttpResponse.Setup(p => p.Cookies)
            .Returns(new Mock<IResponseCookies>().Object);
        _ = mockHttpResponse.Setup(p => p.Body)
            .Returns(streamResponse);

        mockHttpResponse
            .Setup(_ => _.Body.WriteAsync(It.IsAny<byte[]>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Verifiable();

        if (hasNoException)
        {
            _ = mockHttpResponse.SetupGet(p => p.Headers)
                .Returns(new HeaderDictionary());
        }

        _ = mockHttpContext.Setup(p => p.Response)
            .Returns(mockHttpResponse.Object);

        var mockHttpRequest = new Mock<HttpRequest>();
        _ = mockHttpRequest.Setup(p => p.Body).Returns(streamRequest);
        _ = mockHttpRequest.Setup(p => p.Path).Returns("/api/path");
        _ = mockHttpRequest.SetupGet(p => p.Headers)
            .Returns(new HeaderDictionary());

        _ = mockHttpContext.Setup(p => p.Request)
            .Returns(mockHttpRequest.Object);

        var fc = new FeatureCollection();
        var endPoint = new Endpoint(c => Task.CompletedTask, new(), "for unit test");
        var ehf = new ExceptionHandlerFeature();
        fc.Set(endPoint);
        fc.Set(ehf);

        _ = mockHttpContext.Setup(p => p.Features)
            .Returns(fc);

        _ = _json.Setup(p => p.SerializeToString(It.IsAny<ErrorDetails>(), It.IsAny<JsonSerializerOptions>()))
            .Returns("test");
        return mockHttpContext;
    }
}
