using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Core.Models.Messages;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Vip;
using TGS.Services.BackOffice.Application.Models.ReportVip;
using Tgs.ServiceUtility;

namespace TGS.Apis.BackendOffice.Controllers.Report;

[Route("api/[controller]")]
[ApiController]
public class VipReportController : BaseContextController
{
    private readonly IReportClient _client;

    public VipReportController(IReportClient client) => _client = client;

    [HttpGet]
    [Description("Get vips")]
    public async Task<GetVipReportReportResponse> GetVipsAsync(
        [FromServices] IGetService<GetVipReportReportRequest, GetVipReportReportResponse>
            vipReportService,
        [FromQuery] GetVipReportReportRequest request)
        => await vipReportService
            .GetAsync(request)
            .ConfigureAwait(false);

    [HttpGet("{username}")]
    [Description("Get vip rebates")]
    public async Task<PagedResponse<VipRebateReportModel>> GetVipRebatesAsync(string username,
        [FromServices] IGetService<GetVipRebateReportReportRequest, PagedResponse<VipRebateReportModel>>
            vipRebateReportService,
        [FromQuery] GetVipRebateReportReportRequest request)
    {
        request.Username = username;

        return await vipRebateReportService
            .GetAsync(request)
            .ConfigureAwait(false);
    }

    [HttpGet("exportVips")]
    [Description("Export vips")]
    [ByPassAdminLog]
    public async Task<IActionResult> ExportVipsAsync(
        [FromServices] IGetService<ExportVipReportRequest, Stream>
            exportVipReportService,
        [FromQuery] ExportVipReportRequest request) => File(
        await exportVipReportService
            .GetAsync(request)
            .ConfigureAwait(false),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "Vips.xlsx");
}