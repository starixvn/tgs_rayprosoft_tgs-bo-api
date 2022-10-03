using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Services.BackOffice.Application.Models.ReportLicenseeMarket;
using Tgs.ServiceUtility;

namespace TGS.Apis.BackendOffice.Controllers.Report;

[Route("api/[controller]")]
[ApiController]
public class LicenseeMarketReportController : BaseContextController
{
    [HttpGet("exportLicenseeMarkets")]
    [Description(" Export licensee markets")]
    [ByPassAdminLog]
    public async Task<IActionResult> ExportMarketsAsync(
        [FromServices] IGetService<GetLicenseeMarketsReportRequest, Stream>
            exportLicenseeMarketsService,
        [FromQuery] GetLicenseeMarketsReportRequest request)
        => File(
            await exportLicenseeMarketsService
                .GetAsync(request)
                .ConfigureAwait(false),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "LicenseeMarkets.xlsx");

    [HttpGet]
    [Description("Get licensee markets")]
    public async Task<LicenseeMarketsReportResponse> GetLicenseeMarketsAsync(
        [FromServices] IGetService<GetLicenseeMarketsReportRequest, LicenseeMarketsReportResponse>
            licenseeMarketsService,
        [FromQuery] GetLicenseeMarketsReportRequest request)
        => await licenseeMarketsService
            .GetAsync(request)
            .ConfigureAwait(false);
}