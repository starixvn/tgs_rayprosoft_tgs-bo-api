#nullable enable

using System;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sentry;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.CashQueryService.Client;
using Tgs.CashService.Client;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportAgent;
using TGS.Services.BackOffice.Application.Models.ReportPlayer;
using TGS.Services.BackOffice.ApplicationServices.Wallets;
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
    public async Task<PagedResponse<PlayerAccountChangeReportModel>> GetPlayerAccountChangesAsync(
        [FromServices] IPlayerServiceClient playerServiceClient,
        [FromServices] ICashQueryClient cashQueryClient,
        [FromServices] IGetService<AccountChangesQuery, Tgs.CashService.Client.PagingResponse<TransactionLog>> getService,
        [FromQuery] GetPlayerAccountChangesReportRequest request)
    {
        var playerProps = await playerServiceClient
            .GetPlayerPropertiesAsync(request.PlayerCode, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var playerWalletId = Guid.Parse(playerProps["DefaultWalletId"]!);

        var query = new AccountChangesQuery(
            playerWalletId,
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
                TransactionType = (BoEnums.TransactionType)x.TransactionType switch
                {
                    BoEnums.TransactionType.GameProviderFundOutLock => BoEnums.TransactionType.GameProviderFundOut,
                    var type => type
                },
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
    public async Task<PagedResponse<AccountChangeViewModel>> GetPlayerAccountChangesAsync(
        [FromServices] IGetService<AccountChangesQuery, Tgs.CashService.Client.PagingResponse<TransactionLog>> getService,
        [FromServices] ICashQueryClient cashQueryClient,
        [FromServices] IPlayerServiceClient playerServiceClient,
        string playerCode,
        [FromQuery] AccountChangesQueryViewModel conditionsViewModel,
        [FromQuery] PagingViewModel paging)
    {
        var playerProps = await playerServiceClient
            .GetPlayerPropertiesAsync(playerCode, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var playerWalletId = Guid.Parse(playerProps["DefaultWalletId"]!);

        var query = new AccountChangesQuery(
            playerWalletId,
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
            Items = result.Items?.Select(x => new AccountChangeViewModel
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

    [HttpGet("~/api/agent/accountChanges", Name = "GetAgentAccountChangesOld")]
    [Description("Get agent account changes")]
    public async ValueTask<PagedResponse<AgentAccountChangeReportModel>> GetAgentAccountChangesAsync(
        [FromServices] IAgentClient agentClient,
        [FromServices] IGetService<AccountChangesQuery, Tgs.CashService.Client.PagingResponse<TransactionLog>> getService,
        [FromQuery] GetAgentAccountChangesReportRequest request)
    {
        var agent = await agentClient.GetAgentInfoAsync(
            new GetAgentInfoRequest(request.AgentId),
            HttpContext.RequestAborted).ConfigureAwait(false);

        if (agent is null || agent.WalletId is null)
            return new PagedResponse<AgentAccountChangeReportModel>
            {
                Valid = true,
                Items = [],
                TotalCount = 0
            };

        var query = new AccountChangesQuery(
            agent.WalletId.Value,
            request.From.DateTime,
            request.To.DateTime,
            request.TransactionType,
            new PagingConditions(request.Page, request.Rows));

        var result = await getService
            .GetAsync(query, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new PagedResponse<AgentAccountChangeReportModel>
        {
            Valid = true,
            TotalCount = result.TotalCount,
            Items = result.Items.Select(x => new AgentAccountChangeReportModel
            {
                TransactionId = x.Id,
                Amount = x.Amount,
                Balance = x.Balance,
                TransactionType = (BoEnums.TransactionType)x.TransactionType,
                RecordedOn = x.RecordedOn,
            }).ToArray()
        };
    }

    [HttpGet("account-changes/agent/{agentId:Guid}", Name = "GetAgentAccountChanges")]
    public async Task<PagedResponse<AccountChangeViewModel>> GetAgentAccountChangesAsync(
        [FromServices] IGetService<AccountChangesQuery, Tgs.CashService.Client.PagingResponse<TransactionLog>> getService,
        [FromServices] IAgentClient agentClient,
        Guid agentId,
        [FromQuery] AccountChangesQueryViewModel conditionsViewModel,
        [FromQuery] PagingViewModel paging)
    {
        var agent = await agentClient.GetAgentInfoAsync(
            new GetAgentInfoRequest(agentId),
            HttpContext.RequestAborted).ConfigureAwait(false);

        if (agent is null || agent.WalletId is null)
            return new PagedResponse<AccountChangeViewModel>
            {
                Valid = true,
                Items = [],
                TotalCount = 0
            };

        var query = new AccountChangesQuery(
            agent.WalletId.Value,
            conditionsViewModel.From,
            conditionsViewModel.To,
            conditionsViewModel.AccountChangeType,
            new PagingConditions(paging.Page, paging.Rows));

        var result = await getService
            .GetAsync(query, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new()
        {
            Valid = true,
            Message = string.Empty,
            TotalCount = result.TotalCount,
            Items = result.Items?.Select(x => new AccountChangeViewModel
            {
                TransactionId = x.Id,
                Amount = x.Amount,
                LockAmount = x.LockAmount,
                Balance = x.Balance,
                LockedBalance = x.LockedBalance,
                TransactionType = (BoEnums.TransactionType)x.TransactionType,
                RecordedOn = x.RecordedOn,
                IncludeInNetProfit = false,
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

    [HttpGet("account-changes/player/waiting/{playerCode}", Name = "GetPlayerWaitingPersistentAccountChanges")]
    public async Task<PagedResponse<AccountChangeViewModel>> GetPlayerWaitingPersistentAccountChangesAsync(
        [FromServices] IGetService<WaitPersistentAccountChangesQuery, Tgs.CashService.Client.PagingResponse<TransactionLog>> queryService,
        [FromServices] ICashQueryClient cashQueryClient,
        string playerCode,
        [FromQuery] PagingViewModel paging)
    {
        var query = new WaitPersistentAccountChangesQuery(
            playerCode,
            new PagingConditions(paging.Page, paging.Rows));

        var result = await queryService
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
            Items = result.Items?.Select(x => new AccountChangeViewModel
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
                    catch (Exception ex)
                    {
                        SentrySdk.CaptureException(ex);
                        return description;
                    }
                }

            case BoEnums.TransactionType.GameProviderFundOut:
            case BoEnums.TransactionType.GameProviderFundIn:
            case BoEnums.TransactionType.DividendAllAgentRebate:
            case BoEnums.TransactionType.Transfer:
            case BoEnums.TransactionType.GameProviderFundOutLock:
                return description;

            default:
                return string.Empty;
        }
    }
}