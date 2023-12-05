#nullable enable

using System;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.CashQueryService.Client;
using Tgs.CashService.Client;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportPlayer;
using TGS.Services.BackOffice.Application.Queries.PlayerHandlers;
using TGS.Services.BackOffice.ApplicationServices.Players;
using BoEnums = TGS.Services.BackOffice.Domain.Enums;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WalletController : BaseContextController
{
    [HttpGet("~/api/player/{playerCode}/mainwallet", Name = "GetPlayerMainWalletBalanceOld")]
    [HttpGet("player/{playerCode}", Name = "GetPlayerMainWalletBalance")]
    [Description("Get Player Main wallet")]
    public ValueTask<decimal> GetPlayerMainWalletBalanceAsync(
        [FromServices] IGetService<GetPlayerBalanceQuery, decimal> service,
        string playerCode)
        => service.GetAsync(new(playerCode));

    [HttpGet("~/api/player/accountChanges", Name = "GetPlayerAccountChangesOld")]
    [Description("Get player account changes")]
    public async Task<PagedResponse<PlayerAccountChangeReportModel>> GetAccountChangesAsync(
        [FromQuery] GetPlayerAccountChangesReportRequest request,
        [FromServices] IGetService<AccountChangesQuery, PagingResponse<TransactionLog>> getService,
        [FromServices] ICashQueryClient cashQueryClient)
    {
        var query = new AccountChangesQuery(
            request.PlayerCode,
            request.From.DateTime,
            request.To.DateTime,
            request.AccountChangeType,
            new PagingConditions(request.Page, request.Rows));

        var result = await getService
            .GetAsync(query, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var transactionIds = result.Items.Select(tl => tl.Id);

        var adjustPlayerRecords = await cashQueryClient
            .QueryPlayerAdjustIncludeInNetProfitRecordsAsync(transactionIds, HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted);

        return new()
        {
            Valid = true,
            Message = string.Empty,
            TotalCount = result.TotalCount,
            Items = result.Items?.Select(x => new PlayerAccountChangeReportModel
            {
                TransactionId = x.Id,
                GameActionId = null,
                Amount = x.Amount,
                Balance = x.Balance,
                TransactionType = (BoEnums.TransactionType?)x.TransactionType,
                GameActionType = (GameActionType?)x.TransactionType switch
                {
                    GameActionType.PlaceBet => TransactionGameActionType.GameBet,
                    GameActionType.WinBet => TransactionGameActionType.GameSettleWin,
                    GameActionType.LoseBet => TransactionGameActionType.GameSettleLose,
                    GameActionType.FreeBet => TransactionGameActionType.GameSettleFree,
                    GameActionType.TieBet => TransactionGameActionType.GameSettleTie,
                    GameActionType.CancelTransaction => TransactionGameActionType.GameCancel,
                    GameActionType.LockAmount => TransactionGameActionType.GameLock,
                    GameActionType.UnlockAmount => TransactionGameActionType.GameUnlock,
                    GameActionType.WinJackpot => TransactionGameActionType.GameWinJackpot,
                    _ => null
                },
                RecordedOn = x.RecordedOn,
                IncludeInNetProfit = adjustPlayerRecords
                    .FirstOrDefault(apr => apr.TransactionId == x.Id)?.IncludeInNetProfit
                    ?? false,
                Remark = GetRemark((BoEnums.TransactionType?)x.TransactionType, x.Description),
                ExternalTransactionId = x.ExternalId
            }).ToArray()
        };
    }

    [HttpGet("account-changes/player/{playerCode}", Name = "GetPlayerAccountChanges")]
    public async Task<PagedResponse<PlayerAccountChangeViewModel>> GetAccountChangesAsync(
        [FromServices] IGetService<AccountChangesQuery, PagingResponse<TransactionLog>> getService,
        [FromServices] ICashQueryClient cashQueryClient,
        string playerCode,
        [FromQuery] AccountChangesQueryViewModel conditionsViewModel,
        [FromQuery] PagingViewModel paging)
    {
        var query = new AccountChangesQuery(
            playerCode,
            conditionsViewModel.From,
            conditionsViewModel.To,
            conditionsViewModel.AccountChangeType,
            new PagingConditions(paging.Page, paging.Rows));

        var result = await getService
            .GetAsync(query, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var transactionIds = result.Items.Select(tl => tl.Id);

        var adjustPlayerRecords = await cashQueryClient
            .QueryPlayerAdjustIncludeInNetProfitRecordsAsync(transactionIds, HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted);

        return new()
        {
            Valid = true,
            Message = string.Empty,
            TotalCount = result.TotalCount,
            Items = result.Items?.Select(x => new PlayerAccountChangeViewModel
            {
                TransactionId = x.Id,
                Amount = x.Amount,
                LockAmount = x.LockAmount,
                Balance = x.Balance,
                LockedBalance = x.LockedBalance,
                TransactionType = (BoEnums.TransactionType)x.TransactionType,
                RecordedOn = x.RecordedOn,
                IncludeInNetProfit = adjustPlayerRecords
                    .FirstOrDefault(apr => apr.TransactionId == x.Id)?.IncludeInNetProfit
                    ?? false,
                Remark = GetRemark((BoEnums.TransactionType?)x.TransactionType, x.Description),
                ExternalTransactionId = x.ExternalId
            }).ToArray()
        };
    }

    [HttpGet("~/api/player/accountChanges/totalAmount", Name = "GetAccountChangesTotalAmountOld")]
    [Description("Get player account changes Total Amount")]
    public async Task<PlayerAccountChangesTotalModel> GetAccountChangesTotalAmountAsync(
        [FromServices] IGetService<AccountChangesTotalAmountQuery, decimal?> getService,
        [FromQuery] GetPlayerAccountChangesReportTotalAmountRequest request)
    {
        var totalAmount = await getService.GetAsync(
            new AccountChangesTotalAmountQuery(
                request.PlayerCode,
                request.From.DateTime,
                request.To.DateTime,
                request.AccountChangeType),
            HttpContext.RequestAborted).ConfigureAwait(false);

        return new PlayerAccountChangesTotalModel { Amount = totalAmount ?? 0M };
    }

    [HttpGet("account-changes/player/totalAmount/{playerCode}", Name = "GetAccountChangesTotalAmount")]
    public async Task<decimal> GetAccountChangesTotalAmountAsync(
        [FromServices] IGetService<AccountChangesTotalAmountQuery, decimal?> getService,
        string playerCode,
        [FromQuery] AccountChangesQueryViewModel conditionsViewModel)
    {
        var totalAmount = await getService.GetAsync(
            new AccountChangesTotalAmountQuery(
                playerCode,
                conditionsViewModel.From,
                conditionsViewModel.To,
                conditionsViewModel.AccountChangeType),
            HttpContext.RequestAborted).ConfigureAwait(false);

        return totalAmount ?? 0M;
    }

    private static string? GetRemark(BoEnums.TransactionType? transactionType, string? description)
    {
        switch (transactionType)
        {
            case BoEnums.TransactionType.CompanyAdjust:
            case BoEnums.TransactionType.AgentAdjust:
            case BoEnums.TransactionType.SystemAdjust:
            case BoEnums.TransactionType.TransferAdjust:
            case BoEnums.TransactionType.BonusAdjust:
                {
                    if (string.IsNullOrEmpty(description))
                    {
                        return string.Empty;
                    }

                    try
                    {
                        _ = JsonDocument.Parse(description).RootElement.TryGetProperty("Remark", out var remark);
                        return remark.GetString();
                    }
                    catch (Exception)
                    {
                        return description;
                    }
                }

            case BoEnums.TransactionType.GameProviderFundOut:
            case BoEnums.TransactionType.GameProviderFundIn:
            case BoEnums.TransactionType.DividendAllAgentRebate:
            case BoEnums.TransactionType.Transfer:
                return description;

            default:
                return string.Empty;
        }
    }
}
