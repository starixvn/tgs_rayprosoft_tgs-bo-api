using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Services.BackOffice.Application.Models.ReportResellerMarket;

namespace TGS.Apis.BackendOffice.Controllers.Report;

[Route("api/[controller]")]
[ApiController]
public class ResellerMarketReportController : BaseContextController
{
    [HttpGet("export")]
    [Description(" Export reseller markets")]
    [ByPassAdminLog]
    public async Task<IActionResult> ExportResellerMarketsAsync(
        [FromServices] IGetService<GetResellerMarketsReportRequest, Stream>
            exportResellerMarketsService,
        [FromQuery] GetResellerMarketsReportRequest request)
        => File(
            await exportResellerMarketsService
                .GetAsync(request)
                .ConfigureAwait(false),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "ResellerMarkets.xlsx");

    [HttpGet]
    [Description("Get reseller markets")]
    public async Task<ResellerMarketsReportResponse> GetResellerMarketsAsync(
        [FromServices] IGetService<GetResellerMarketsReportRequest, ResellerMarketsReportResponse>
            resellerMarketsService,
        [FromQuery] GetResellerMarketsReportRequest request)
        => await resellerMarketsService
            .GetAsync(request)
            .ConfigureAwait(false);
}
