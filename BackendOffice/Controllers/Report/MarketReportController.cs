using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.Client.Models.Market;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportMarket;

namespace TGS.Apis.BackendOffice.Controllers.Report;

[Route("api/[controller]")]
[ApiController]
public class MarketReportController : BaseContextController
{
    [HttpGet("exportMarkets")]
    [Description("Export markets")]
    [ByPassAdminLog]
    public async Task<IActionResult> ExportMarketsAsync(
        [FromServices] IGetService<ExportMarketsRequest, Stream>
            exportMarketsService,
        [FromQuery] ExportMarketsRequest request) => File(
        await exportMarketsService
            .GetAsync(request)
            .ConfigureAwait(false),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "Markets.xlsx");

    [HttpGet("additionalExpenses")]
    [Description("Get Additional Expenses")]
    public async Task<GetAdditionalExpensesReportResponse> GetAdditionalExpensesAsync(
        [FromServices] IGetService<GetAdditionalExpensesReportRequest, GetAdditionalExpensesReportResponse>
            additionalExpensesService,
        [FromQuery] GetAdditionalExpensesReportRequest request)
        => await additionalExpensesService
            .GetAsync(request)
            .ConfigureAwait(false);

    [HttpGet]
    [Description("Get market report")]
    public async Task<GetMarketsReportResponse> GetMarketsAsync(
        [FromServices] IGetService<GetMarketsReportRequest, GetMarketsReportResponse>
            marketsService,
        [FromQuery] GetMarketsReportRequest request)
        => await marketsService
            .GetAsync(request)
            .ConfigureAwait(false);

    [HttpGet("agentProfit")]
    [Description("Get agent profits report")]
    public async Task<PagedResponse<MarketAgentProfitReportModel>> GetMarketAgentProfitAsync(
        [FromServices] IGetService<GetMarketAgentProfitsReportRequest, PagedResponse<MarketAgentProfitReportModel>>
            marketsService,
        [FromQuery] GetMarketAgentProfitsReportRequest request)
        => await marketsService
            .GetAsync(request)
            .ConfigureAwait(false);
}
