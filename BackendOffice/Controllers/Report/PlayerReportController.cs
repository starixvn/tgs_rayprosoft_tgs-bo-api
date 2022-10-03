using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Core.Models.Messages;
using Tgs.Report.Client.Models.Player;
using TGS.Services.BackOffice.Application.Models.ReportPlayer;
using Tgs.ServiceUtility;

namespace TGS.Apis.BackendOffice.Controllers.Report;

[Route("api/[controller]")]
[ApiController]
public class PlayerReportController : BaseContextController
{
    [HttpGet("contribution")]
    [Description("Get agent contributions")]
    public async Task<GetPlayerContributionsReportResponse> GetPlayerContributionsAsync(
        [FromServices] IGetService<GetPlayerContributionsReportRequest, GetPlayerContributionsReportResponse>
            playerContributionsService,
        [FromQuery] GetPlayerContributionsReportRequest request)
        => await playerContributionsService
            .GetAsync(request)
            .ConfigureAwait(false);

    [HttpGet("playersAccountChanges")]
    [Description("Get account changes of multiple players")]
    public async Task<PagedResponse<PlayersAccountChangeReportModel>> GetPlayersAccountChangesAsync(
        [FromServices]
        IGetService<GetPlayersAccountChangesReportRequest, PagedResponse<PlayersAccountChangeReportModel>>
            playersAccountChangesService,
        [FromQuery] GetPlayersAccountChangesReportRequest request)
        => await playersAccountChangesService
            .GetAsync(request)
            .ConfigureAwait(false);

    [HttpGet("exportPlayerContributions")]
    [Description("Export player contributions")]
    [ByPassAdminLog]
    public async Task<IActionResult> ExportPlayerContributionsAsync(
        [FromServices] IGetService<ExportPlayerContributionsRequest, Stream>
            exportPlayerContributionsService,
        [FromQuery] ExportPlayerContributionsRequest request)
        => File(
            await exportPlayerContributionsService
                .GetAsync(request)
                .ConfigureAwait(false),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "PlayerContributions.xlsx");
}