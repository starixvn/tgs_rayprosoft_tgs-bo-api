using System.Data.SqlClient;
using AutoFixture;
using FluentAssertions;
using Moq;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Report;
using TGS.Services.BackOffice.Application.Models.ReportMarket;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class MarketControllerTests : BaseTests
{
    private readonly Mock<IGetService<GetMarketsReportRequest, GetMarketsReportResponse>> _getServiceMock =
        new();

    private MarketReportController _controller = default!;

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Markets(bool showAll)
    {
        var list = new Fixture()
            .CreateMany<MarketReportModel>(99)
            .ToArray();

        var request = new GetMarketsReportRequest
        {
            Page = 1,
            Rows = 10,
            SortBy = "updatedOn",
            OrderBy = SortOrder.Descending,
            ShowAll = showAll
        };
        var (skip, take) = GetPage(request);
        var orderedRoles = list.OrderByDescending(p => p.ReportDate);

        var expectedRoles = showAll ? orderedRoles.ToArray() : orderedRoles.Skip(skip).Take(take).ToArray();

        _ = _getServiceMock
            .Setup(m => m.GetAsync(It.IsAny<GetMarketsReportRequest>(), default))
            .ReturnsAsync(new GetMarketsReportResponse
            {
                TotalCount = list.Length,
                Items = expectedRoles
            });

        var response = await _controller.GetMarketsAsync(_getServiceMock.Object, request);

        for (var i = 0; i < response.Items.Length; i++)
        {
            _ = response.Items[i].ReportDate.Should().Be(expectedRoles[i].ReportDate);
        }

        _getServiceMock.Verify(x => x.GetAsync(It.IsAny<GetMarketsReportRequest>(), default));
    }

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _controller = new();
    }
}
