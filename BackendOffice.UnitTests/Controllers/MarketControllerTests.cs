using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using TGS.Apis.BackendOffice.Controllers.Report;
using TGS.Services.BackOffice.Application.Models.ReportMarket;
using Tgs.ServiceUtility;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class MarketControllerTests : BaseTests
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _controller = new();
    }

    private MarketReportController _controller = default!;

    private readonly Mock<IGetService<GetMarketsReportRequest, GetMarketsReportResponse>> _getServiceMock =
        new();

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

        _getServiceMock
            .Setup(m => m.GetAsync(It.IsAny<GetMarketsReportRequest>(), default))
            .ReturnsAsync(new GetMarketsReportResponse
            {
                TotalCount = list.Count(),
                Items = expectedRoles
            });

        var response = await _controller.GetMarketsAsync(_getServiceMock.Object, request);

        for (var i = 0; i < response.Items.Length; i++)
        {
            response.Items[i].ReportDate.Should().Be(expectedRoles[i].ReportDate);
        }

        _getServiceMock.Verify(x => x.GetAsync(It.IsAny<GetMarketsReportRequest>(), default));
    }
}