using AutoFixture;
using FluentAssertions;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Report;
using TGS.Services.BackOffice.Application.Models.ReportMarket;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class MarketControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Markets(bool showAll)
    {
        var getServiceMock = Substitute.For<IGetService<GetMarketsReportRequest, GetMarketsReportResponse>>();

        var controller = new MarketReportController { ControllerContext = { HttpContext = HttpContextMock } };

        var list = new Fixture()
            .CreateMany<MarketReportModel>(99)
            .ToArray();

        var request = new GetMarketsReportRequest
        {
            Page = 1,
            Rows = 10,
            SortBy = "updatedOn",
            OrderBy = Core.Models.Messages.SortOrder.Descending,
            ShowAll = showAll
        };
        var (skip, take) = GetPage(request);
        var orderedRoles = list.OrderByDescending(p => p.ReportDate);

        var expectedRoles = showAll ? [.. orderedRoles] : orderedRoles.Skip(skip).Take(take).ToArray();

        _ = getServiceMock
            .GetAsync(Arg.Any<GetMarketsReportRequest>())
            .Returns(new GetMarketsReportResponse
            {
                TotalCount = list.Length,
                Items = expectedRoles
            });

        var response = await controller.GetMarketsAsync(getServiceMock, request);

        for (var i = 0; i < response.Items.Length; i++)
            _ = response.Items[i].ReportDate.Should().Be(expectedRoles[i].ReportDate);

        _ = getServiceMock.Received(1).GetAsync(Arg.Any<GetMarketsReportRequest>());
    }
}