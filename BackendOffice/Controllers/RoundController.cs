using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.GameProviders.Pluggable.Client;
using Tgs.GameProviders.Pluggable.ViewModels;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Round;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Apis.BackendOffice.Controllers.Messages.Round;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.ReportRound;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoundController(IReportClient client,
    IGameProvidersBackofficePluggableClient gameProvidersBackofficePluggableClient) : BaseContextController
{
    [HttpPut("{roundId}/AdjustClosedRound", Name = "AdjustClosedRound")]
    [BOAuthorize(["SystemAdmin.ResettleGameRound.Update"])]
    [Description("Adjust a Closed round")]
    public async Task<MessageResponse> AdjustClosedRoundAsync(Guid roundId, AdjustClosedRoundRequest request)
    {
        var model = new AdjustClosedRoundViewModel
        {
            BetAmount = request.BetAmount,
            ClientRoundStatus = request.ClientRoundStatus,
            Commission = request.Commission,
            ExternalClosedOn = request.ExternalClosedOn,
            OperatorId = Guid.Parse(UserId),
            Remark = request.Remark,
            SettleAmount = request.SettleAmount,
            Turnover = request.Turnover,
            ValidBetAmount = request.ValidBetAmount,
            WinLoss = request.WinLoss,
            AdjustAmount = request.AdjustAmount
        };

        await gameProvidersBackofficePluggableClient.AdjustClosedRoundAsync(roundId, model).ConfigureAwait(false);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpPut("{roundId}/CloseGameRound", Name = "CloseGameRound")]
    [BOAuthorize(["SystemAdmin.GameRound.Update"])]
    [Description("Close a game round")]
    public async Task<MessageResponse> CloseGameRoundAsync(Guid roundId, CloseGameRoundRequest request)
    {
        var model = new GameRoundClosedViewModel
        {
            BetAmount = request.BetAmount,
            ClientRoundStatus = request.ClientRoundStatus,
            Commission = request.Commission,
            ExternalClosedOn = request.ExternalClosedOn,
            OperatorId = Guid.Parse(UserId),
            Remark = request.Remark,
            SettleAmount = request.SettleAmount,
            Turnover = request.Turnover,
            ValidBetAmount = request.ValidBetAmount,
            WinLoss = request.WinLoss,
            AdjustAmount = request.AdjustAmount
        };

        await gameProvidersBackofficePluggableClient.GameRoundClosedAsync(roundId, model).ConfigureAwait(false);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpGet("openRounds", Name = "GetOpenRounds")]
    [BOAuthorize(["SystemAdmin.GameRound"])]
    [Description("Get open rounds")]
    public async Task<PagedResponse<OpenRoundReportModel>> GetOpenRoundsAsync(
        [FromQuery] GetOpenRoundsReportRequest request)
    {
        var reportRequest = new GetOpenRoundsRequest
        {
            ExternalRoundId = request.ExternalRoundId,
            GameProviderId = request.GameProviderId,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };

        var response = await client.GetOpenRoundsAsync(reportRequest);

        return new()
        {
            TotalCount = response.TotalCount,
            Items = response.Items.Select(x => new OpenRoundReportModel
            {
                Id = x.Id,
                AdditionalData = x.AdditionalData,
                BetAmount = x.BetAmount,
                BrandCode = x.BrandCode,
                BrandName = x.BrandName,
                ClosedOn = x.ClosedOn,
                Commission = x.Commission ?? 0,
                Turnover = x.Turnover,
                CreatedOn = x.CreatedOn,
                CurrencyCode = x.CurrencyCode,
                ExternalClosedOn = x.ExternalClosedOn,
                ExternalCreatedOn = x.ExternalCreatedOn,
                ExternalPlayerCode = x.ExternalPlayerCode,
                ExternalRoundId = x.ExternalRoundId,
                GameId = x.GameId,
                GameName = x.GameName,
                GameProviderCode = x.GameProviderCode,
                GameProviderId = x.GameProviderId,
                GameProviderName = x.GameProviderName,
                PlatformType = (PlatformType)x.PlatformType,
                PlayerCode = x.PlayerCode,
                PlayerName = x.PlayerName,
                PlayerType = (PlayerType)x.PlayerType,
                SettleAmount = x.SettleAmount,
                Status = (ClientRoundStatus)x.Status,
                SubGameProviderName = x.SubGameProviderName,
                TieBetAdjustment = x.TieBetAdjustment,
                UpdatedOn = x.UpdatedOn,
                ValidBetAmount = x.ValidBetAmount,
                WinLoss = x.WinLoss,
            }).ToArray()
        };
    }
}