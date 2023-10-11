using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.Client.Models.Player;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportPlayer;

namespace TGS.Apis.BackendOffice.Controllers.Report;

[Route("api/[controller]")]
[ApiController]
public class PlayerReportController : BaseContextController
{
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

    [HttpGet("player-all-agent-rebate-total-amount")]
    [Description("Get player all agent rebate total amount")]
    public async Task<PlayerAllAgentRebateTotalAmountReportModel> GetPlayerAllAgentRebateTotalAmountAsync(
        [FromServices]
        IGetService<GetPlayerAllAgentRebateTotalAmountReportRequest, PlayerAllAgentRebateTotalAmountReportModel>
            playerAllAgentRebateTotalAmountService,
        [FromQuery] GetPlayerAllAgentRebateTotalAmountReportRequest request)
        => await playerAllAgentRebateTotalAmountService
            .GetAsync(request)
            .ConfigureAwait(false);

    [HttpGet("player-all-agent-rebates")]
    [Description("Get player all agent rebates")]
    public IAsyncEnumerable<PlayerAllAgentRebateReportModel> GetPlayerAllAgentRebatesAsync(
        [FromServices]
        IQueryService<GetPlayerAllAgentRebatesReportRequest, PlayerAllAgentRebateReportModel>
            playerAllAgentRebatesService,
        [FromQuery] GetPlayerAllAgentRebatesReportRequest request)
        => playerAllAgentRebatesService
            .QueryAsync(request);

    [HttpGet("player-all-agent-rebate-details")]
    [Description("Get player all agent rebate details")]
    public IAsyncEnumerable<PlayerAllAgentRebateDetailReportModel> GetPlayerAllAgentRebateDetailsAsync(
        [FromServices]
        IQueryService<GetPlayerAllAgentRebatesDetailsReportRequest, PlayerAllAgentRebateDetailReportModel>
            playerAllAgentRebateDetailService,
        [FromQuery] GetPlayerAllAgentRebatesDetailsReportRequest request)
        => playerAllAgentRebateDetailService
            .QueryAsync(request);

    [HttpGet("player-all-agent-rebate-detail-game-types")]
    [Description("Get player all agent rebate detail game types")]
    public IAsyncEnumerable<PlayerAllAgentRebateDetailGameTypeReportModel> GetPlayerAllAgentRebateDetailGameTypesAsync(
        [FromServices]
        IQueryService<GetPlayerAllAgentRebateDetailGameTypesReportRequest, PlayerAllAgentRebateDetailGameTypeReportModel>
            playerAllAgentRebateDetailGameTypeService,
        [FromQuery] GetPlayerAllAgentRebateDetailGameTypesReportRequest request)
        => playerAllAgentRebateDetailGameTypeService
            .QueryAsync(request);
}
