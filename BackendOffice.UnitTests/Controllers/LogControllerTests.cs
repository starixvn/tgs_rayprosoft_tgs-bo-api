using System.Data.SqlClient;
using AutoFixture;
using FluentAssertions;
using Moq;
using TGS.Apis.BackendOffice.Controllers;
using TGS.Apis.BackendOffice.Controllers.Messages.Log;
using TGS.Apis.BackendOffice.DataService.Repositories;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class LogControllerTests : BaseTests
{
    private readonly Mock<ILogQueries> _logQueriesMock = new();

    private LogController _controller = default!;

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Admin_Logs(bool showAll)
    {
        var request = new AdminLogRequest
        {
            Page = 1,
            Rows = 10,
            SortBy = "recordedOn",
            OrderBy = SortOrder.Descending,
            ShowAll = showAll
        };

        var list = CreateFixture<AdminLogModel>()
            .CreateMany<AdminLogModel>(10)
            .AsEnumerable();

        _ = _logQueriesMock.Setup(x => x.GetAdminLogsAsync(It.IsAny<AdminLogRequest>()))
            .Returns(Task.FromResult((list, list.Count())));

        var array = list.ToArray();
        var response = await _controller.GetAdminLogsAsync(request);
        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            _ = response.Items[i].Id.Should().Be(array[i].Id);
            _ = response.Items[i].TrackingId.Should().Be(array[i].TrackingId);
        }

        _logQueriesMock.Verify(p => p.GetAdminLogsAsync(It.IsAny<AdminLogRequest>()));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Api_Logs(bool showAll)
    {
        var request = new ApiLogRequest
        {
            Page = 1,
            Rows = 10,
            SortBy = "recordedOn",
            OrderBy = SortOrder.Descending,
            ShowAll = showAll
        };

        var list = CreateFixture<ApiLogModel>()
            .CreateMany<ApiLogModel>(10)
            .AsEnumerable();

        _ = _logQueriesMock.Setup(x => x.GetApiLogsAsync(It.IsAny<ApiLogRequest>()))
            .Returns(Task.FromResult((list, list.Count())));

        var array = list.ToArray();
        var response = await _controller.GetApiLogsAsync(request);
        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            _ = response.Items[i].Id.Should().Be(array[i].Id);
            _ = response.Items[i].TrackingId.Should().Be(array[i].TrackingId);
        }

        _logQueriesMock.Verify(p => p.GetApiLogsAsync(It.IsAny<ApiLogRequest>()));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Log_Records(bool showAll)
    {
        var request = new LogRecordRequest
        {
            Page = 1,
            Rows = 10,
            SortBy = "recordedOn",
            OrderBy = SortOrder.Descending,
            ShowAll = showAll
        };

        var list = CreateFixture<LogRecordModel>()
            .CreateMany<LogRecordModel>(10)
            .AsEnumerable();

        _ = _logQueriesMock.Setup(x => x.GetLogRecordsAsync(It.IsAny<LogRecordRequest>()))
            .Returns(Task.FromResult((list, list.Count())));

        var array = list.ToArray();
        var response = await _controller.GetLogRecordsAsync(request);
        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            _ = response.Items[i].Id.Should().Be(array[i].Id);
            _ = response.Items[i].TrackingId.Should().Be(array[i].TrackingId);
        }

        _logQueriesMock.Verify(p => p.GetLogRecordsAsync(It.IsAny<LogRecordRequest>()));
    }

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _controller = new LogController(
            _logQueriesMock.Object);
        _controller.ControllerContext.HttpContext = HttpContextMock.Object;
    }

    private static Fixture CreateFixture<T>()
    {
        var fixture = new Fixture();
        _ = fixture.Create<T>();
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}
