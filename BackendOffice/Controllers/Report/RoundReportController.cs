#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.GameProviders.Pluggable.Client;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Round;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Apis.BackendOffice.DataService.Services;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportRound;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Apis.BackendOffice.Controllers.Report;

[Route("api/[controller]")]
[ApiController]
public class RoundReportController() : BaseContextController
{
    [HttpPut("system/addround")]
    [Description("Add round")]
    public ValueTask AddCompanyRoundAsync(
        [FromServices] ICommandService<AddCompanyRoundCommand> commandService,
        [FromBody] AddCompanyRoundRequest request)
        => commandService
            .ExecuteAsync(new AddCompanyRoundCommand
            {
                PlayerCode = request.PlayerCode,
                ClientRoundStatus = request.ClientRoundStatus,
                GameProviderId = request.GameProviderId,
                GameId = request.GameId,
                ExternalRoundId = request.ExternalRoundId,
                ExternalCreatedOn = request.ExternalCreatedOn,
                ExternalClosedOn = request.ExternalClosedOn,
                BetAmount = request.BetAmount,
                ValidBetAmount = request.ValidBetAmount,
                SettleAmount = request.SettleAmount,
                Turnover = request.Turnover,
                Commission = request.Commission,
                CompanyWinLoss = request.CompanyWinLoss,
                OperatorId = UserId,
                Remark = request.Remark
            }, HttpContext.RequestAborted);

    [HttpGet("exportSystemRounds")]
    [Description("Export system rounds")]
    [ByPassAdminLog]
    public async Task<IActionResult> ExportSystemRoundsAsync(
        [FromServices] IReportClient client,
        [FromQuery] ExportSystemRoundsRequest request)
        => File(await client
                .ExportSystemRoundsAsync(request)
                .ConfigureAwait(false),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "SystemRounds.xlsx");

    [HttpGet("exportRounds")]
    [Description("Export rounds")]
    [ByPassAdminLog]
    public async Task<IActionResult> ExportRoundsAsync(
        [FromServices] IReportClient client,
        [FromQuery] ExportRoundsRequest request)
        => File(await client
                .ExportRoundsAsync(request)
                .ConfigureAwait(false),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Rounds.xlsx");

    [HttpGet("betRecords")]
    [Description("Get bet records")]
    public async ValueTask<GetBetRecordsReportResponse> GetBetRecordsAsync(
        [FromServices] IGetService<GetBetRecordsReportRequest, GetBetRecordsReportResponse>
            betRecordsService,
        [FromQuery] GetBetRecordsReportRequest request)
        => await betRecordsService
            .GetAsync(request)
            .ConfigureAwait(false);

    [HttpGet("closedRounds")]
    [Description("Get closed rounds")]
    public async Task<PagedResponse<ClosedRoundReportModel>> GetClosedRoundsAsync(
        [FromServices] IGetService<GetClosedRoundsReportRequest, PagedResponse<ClosedRoundReportModel>>
            closedRoundsService,
        [FromQuery] GetClosedRoundsReportRequest request)
        => await closedRoundsService
            .GetAsync(request)
            .ConfigureAwait(false);

    [HttpGet("openRounds")]
    [Description("Get open rounds")]
    public async Task<PagedResponse<OpenRoundReportModel>> GetOpenRoundsAsync(
        [FromServices] IGetService<GetOpenRoundsReportRequest, PagedResponse<OpenRoundReportModel>>
            openRoundsService,
        [FromQuery] GetOpenRoundsReportRequest request)
        => await openRoundsService
            .GetAsync(request)
            .ConfigureAwait(false);

    [HttpGet("{roundId}")]
    [Description("Get a round")]
    public async Task<IActionResult> GetRoundHistoryAsync(Guid roundId,
        [FromServices] IGetService<GetRoundHistoryReportRequest, JsonDocument?> roundHistoryService,
        [FromServices] IGetService<GetRoundHistoryReportRequest, GameRoundDetailResult> roundDetailService,
        [FromQuery] GetRoundHistoryReportRequest request)
    {
        var result = await roundDetailService.GetAsync(request);
        if (result is not null)
        {
            return Ok(result);
        }

        request.RoundId = roundId;
        return Ok(await roundHistoryService.GetAsync(request).ConfigureAwait(false));
    }

    [HttpGet]
    [Description("Get rounds")]
    public async ValueTask<TGS.Services.BackOffice.Domain.PagedResponse<RoundView>> GetRoundsAsync(
        [FromServices] IGetService<GetRoundsReportRequest, TGS.Services.BackOffice.Domain.PagedResponse<RoundView>> roundsService,
        [FromQuery] GetRoundsReportRequest request)
        => await roundsService
            .GetAsync(request)
            .ConfigureAwait(false);

    [HttpGet("system")]
    [Description("Get System rounds")]
    public async ValueTask<TGS.Services.BackOffice.Domain.PagedResponse<SystemRoundView>> GetSystemRoundsAsync(
        [FromServices] IGetService<GetSystemRoundsReportRequest, TGS.Services.BackOffice.Domain.PagedResponse<SystemRoundView>> roundsService,
        [FromQuery] GetSystemRoundsReportRequest request)
        => await roundsService
            .GetAsync(request)
            .ConfigureAwait(false);

    [HttpGet("system/total", Name = "GetSystemRoundsTotal")]
    [Description("Get rounds total about BetAmount & ValidBetAmount & Turnover & SettleAmount & WinLoss")]
    public async Task<SystemRoundTotalModel> GetSystemRoundsTotalAsync(
        [FromServices] IReportClient client,
        [FromQuery] GetSystemRoundTotalReportRequest request)
    {
        var roundsRequest = new GetSystemRoundsTotalRequest
        {
            Username = request.Username,
            PlayerCode = request.PlayerCode,
            ExternalRoundId = request.ExternalRoundId,
            RoundId = request.RoundId,
            ExternalFrom = request.ExternalFrom,
            ExternalTo = request.ExternalTo,
            ExternalClosedFrom = request.ExternalClosedFrom,
            ExternalClosedTo = request.ExternalClosedTo,
            From = request.From,
            To = request.To,
            ClosedFrom = request.ClosedFrom,
            ClosedTo = request.ClosedTo,
            Status = (short?)request.Status,
            BrandId = request.BrandId,
            GameProviderId = request.GameProviderId,
            SubGameProviderCode = request.SubGameProviderCode,
            GameType = (short?)request.GameType,
            CurrencyCode = request.CurrencyCode
        };

        var response = await client.GetSystemRoundsTotalAsync(roundsRequest);

        return new SystemRoundTotalModel
        {
            BetAmount = response.BetAmount,
            ValidBetAmount = response.ValidBetAmount,
            Turnover = response.Turnover,
            SettleAmount = response.SettleAmount,
            WinLoss = response.WinLoss
        };
    }
}