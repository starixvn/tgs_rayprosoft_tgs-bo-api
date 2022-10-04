using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tgs.Paging;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using Tgs.Promotion.Clients;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Player;
using Tgs.ServiceUtility;
using Tgs.VipLevelService.Clients;
using TGS.Apis.BackendOffice.Controllers.Messages.Promotion;
using TGS.Apis.BackendOffice.Models;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Application.Models.PlayerBucket;
using TGS.Services.BackOffice.Application.Models.ReportPlayer;
using TGS.Services.BackOffice.Application.Queries.PlayerHandlers;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public partial class PlayerController : BaseContextController
{
    private readonly IPromotionClient _client;
    private readonly ILogger<PlayerController> _logger;
    private readonly IMediator _mediator;

    public PlayerController(IMediator mediator, IPromotionClient client, ILogger<PlayerController> logger)
    {
        _mediator = mediator;
        _client = client;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPut("{playerCode}/clear", Name = "ClearPlayerBuckets")]
    [Description("Clear player buckets")]
    public Task<ClearPlayerBucketsResponse> ClearPlayerBucketsAsync(string playerCode) =>
        _mediator.Send(new ClearPlayerBucketsRequest(playerCode));

    [HttpPost("{playerCode}/adjust/increase", Name = "CreatePlayerIncreaseAmount")]
    [Description("Create a player adjustment to increase amount")]
    public async Task<MessageResponse> CreatePlayerIncreaseAmountAsync(
        [FromServices] ICommandService<AdjustPlayerBalanceCommand> adjustService,
        string playerCode,
        CreatePlayerIncreaseAmountRequest request)
    {
        if (!Enum.IsDefined(request.TransactionType) ||
            !Enum.IsDefined((PlayerAdjustmentTransactionType)request.TransactionType))
            return new MessageResponse()
            {
                Valid = false,
                Message = "is not define type"
            };

        try
        {
            var command = new AdjustPlayerBalanceCommand(
                request.PlayerCode,
                AdjustmentType.Add,
                (PlayerAdjustmentTransactionType)request.TransactionType,
                request.Amount,
                request.BucketAmount,
                request.Remark,
                request.IncludeInNetProfit,
                Guid.Parse(UserId),
                Username);

            await adjustService
                .ExecuteAsync(command, HttpContext.RequestAborted)
                .ConfigureAwait(false);

            return new MessageResponse()
            {
                Valid = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Adjust Fail");

            return new MessageResponse
            {
                Valid = false,
                Message = ex.Message
            };
        }
    }

    [HttpPost("{playerCode}/adjust/reduce", Name = "CreatePlayerReduceAmount")]
    [Description("Create a player adjustment to reduce amount")]
    public async Task<MessageResponse> CreatePlayerReduceAmountAsync(
        [FromServices] ICommandService<AdjustPlayerBalanceCommand> adjustService,
        [FromServices] ICommandService<PlayerTransferBalanceToParentAgentCommand> transferService,
        string playerCode,
        CreatePlayerReduceAmountRequest request)
    {
        if (!Enum.IsDefined(request.TransactionType) ||
            !Enum.IsDefined((PlayerAdjustmentTransactionType)request.TransactionType))
            return new MessageResponse()
            {
                Valid = false,
                Message = "is not define type"
            };

        try
        {
            if (request.TransactionType == PlayerReduceTransactionType.TransferAdjust)
            {
                var command = new PlayerTransferBalanceToParentAgentCommand(
                    request.PlayerCode,
                    request.Amount,
                    request.BucketAmount,
                    request.Remark,
                    request.IncludeInNetProfit,
                    Guid.Parse(UserId));

                await transferService
                    .ExecuteAsync(command, HttpContext.RequestAborted)
                    .ConfigureAwait(false);
            }
            else
            {
                var command = new AdjustPlayerBalanceCommand(
                    request.PlayerCode,
                    AdjustmentType.Subtract,
                    (PlayerAdjustmentTransactionType)request.TransactionType,
                    request.Amount,
                    request.BucketAmount,
                    request.Remark,
                    request.IncludeInNetProfit,
                    Guid.Parse(UserId),
                    Username);

                await adjustService
                    .ExecuteAsync(command, HttpContext.RequestAborted)
                    .ConfigureAwait(false);
            }

            return new MessageResponse()
            {
                Valid = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Adjust Fail");

            return new MessageResponse
            {
                Valid = false,
                Message = ex.Message
            };
        }
    }

    [HttpPost("{playerCode}/fundin/all", Name = "FundInAllPlayerGameProvider")]
    [Description("FundIn All Player GameProvider")]
    public async ValueTask<MessageResponse> FundInAllPlayerGameProviderAsync(
        [FromServices] ICommandService<FundInAllPlayerGameProviderCommand> service,
        string playerCode)
    {
        await service.ExecuteAsync(new FundInAllPlayerGameProviderCommand(
            PlayerCode: playerCode)).ConfigureAwait(false);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpPost("{playerCode}/fundin/", Name = "FundInPlayerGameProvider")]
    [Description("FundIn Player GameProvider")]
    public async ValueTask<MessageResponse> FundInPlayerGameProviderAsync(
        [FromServices] ICommandService<FundInPlayerGameProviderCommand> service,
        string playerCode,
        FundInPlayerGameProviderModel model)
    {
        await service.ExecuteAsync(new FundInPlayerGameProviderCommand(
            PlayerCode: playerCode,
            GameProviderCode: model.GameProviderCode)).ConfigureAwait(false);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpGet("accountChanges", Name = "GetPlayerAccountChanges")]
    [Description("Get player account changes")]
    public async Task<PagedResponse<PlayerAccountChangeReportModel>> GetAccountChangesAsync(
        [FromServices] IReportClient client,
        [FromQuery] GetPlayerAccountChangesReportRequest request)
    {
        var reportRequest = new GetPlayerAccountChangesRequest
        {
            BrandId = request.BrandId,
            PlayerCode = request.PlayerCode,
            AccountChangeType = (short?)request.AccountChangeType,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };
        var response = await client.GetPlayerAccountChangesAsync(reportRequest);

        return new()
        {
            Valid = response.Valid,
            Message = response.Message,
            TotalCount = response.TotalCount,
            Items = response.Items?.Select(x => new PlayerAccountChangeReportModel
            {
                TransactionId = x.TransactionId,
                GameActionId = x.GameActionId,
                Amount = x.Amount,
                Balance = x.Balance,
                TransactionType = (TransactionType?)x.TransactionType,
                GameActionType = (GameActionType?)x.GameActionType switch
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
                IncludeInNetProfit = x.IncludeInNetProfit,
                Remark = GetRemark((TransactionType?)x.TransactionType, x.Description),
                ExternalTransactionId = x.ExternalTransactionId
            }).ToArray()
        };
    }

    [HttpGet("accountChanges/totalAmount", Name = "GetAccountChangesTotalAmount")]
    [Description("Get player account changes Total Amount")]
    public async Task<PlayerAccountChangesTotalModel> GetAccountChangesTotalAmountAsync(
        [FromServices] IReportClient client,
        [FromQuery] GetPlayerAccountChangesReportTotalAmountRequest request)
    {
        var reportRequest = new GetPlayerAccountChangesTotalRequest
        {
            BrandId = request.BrandId,
            PlayerCode = request.PlayerCode,
            AccountChangeType = (short?)request.AccountChangeType,
            From = request.From,
            To = request.To
        };
        var response = await client.GetPlayerAccountChangesTotalAmountAsync(reportRequest);

        return new PlayerAccountChangesTotalModel { Amount = response };
    }

    [HttpGet("{playerCode}/joinedPromotions", Name = "GetJoinedPromotions")]
    [Description("Get player operation")]
    public async Task<PagedResponse<JoinedPromotionModel>> GetJoinedPromotionsAsync(
        string playerCode,
        [FromQuery] GetJoinedPromotionsRequest request)
    {
        var promotionRequest = new Tgs.Promotion.Clients.Models.Promotion.GetJoinedPromotionsRequest
        {
            PlayerCode = playerCode,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };

        var response = await _client.GetJoinedPromotionsAsync(promotionRequest);

        return new PagedResponse<JoinedPromotionModel>
        {
            Valid = response.Valid,
            Message = response.Message,
            TotalCount = response.TotalCount,
            Items = response.Items.Select(x => new JoinedPromotionModel
            {
                PromotionName = x.PromotionName,
                CreatedOn = x.CreatedOn
            }).ToArray()
        };
    }

    [HttpGet(Name = "GetPlayerList")]
    [Description("Get player list")]
    public async Task<PagedResponse<PlayersModel>> GetListAsync(
        [FromServices] IPlayerServiceClient playerServiceClient,
        [FromServices] IPaymentServiceClient paymentServiceClient,
        [FromServices] IQueryService<AllAgentsByBrandQuery, AgentInfo> queryAgentsService,
        [FromServices] IQueryService<AllVipLevelsQuery, VipLevelInfo> queryVipService,
        [FromQuery] GetPlayersRequest request)
    {
        var agentQuery = queryAgentsService
            .QueryAsync(new AllAgentsByBrandQuery(request.BrandId), HttpContext.RequestAborted);

        var agentDict = await agentQuery
            .ToDictionaryAsync(
                a => a.UserName,
                a => a.AgentId,
                HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var agentId = string.IsNullOrWhiteSpace(request.AgentName)
            ? (Guid?)null
            : agentDict.TryGetValue(request.AgentName, out var id)
                ? id
                : Guid.Empty;

        var parentPlayer = !string.IsNullOrWhiteSpace(request.ParentUsername)
            ? await playerServiceClient
                .GetPlayerInfoByUserNameAsync(request.BrandId, request.ParentUsername, HttpContext.RequestAborted)
                .ConfigureAwait(false) ?? new Tgs.PlayerService.Client.PlayerInfo
                {
                    PlayerCode = string.Empty
                }
            : null;

        if (agentId == Guid.Empty || parentPlayer?.PlayerCode == string.Empty)
            return new PagedResponse<PlayersModel>
            {
                Valid = true,
                TotalCount = 0,
                Items = Array.Empty<PlayersModel>()
            };

        string[] bankInfoPlayers = Array.Empty<string>();

        if (!string.IsNullOrWhiteSpace(request.BankInfoTagType) && !string.IsNullOrWhiteSpace(request.BankInfoTag))
        {
            bankInfoPlayers = await paymentServiceClient
                .GetPlayersByBankInfoTagAsync(request.BrandId, request.BankInfoTagType, request.BankInfoTag)
                .ToArrayAsync(HttpContext.RequestAborted)
                .ConfigureAwait(false);

            if (bankInfoPlayers.Length == 0)
                return new PagedResponse<PlayersModel>
                {
                    Valid = true,
                    TotalCount = 0,
                    Items = Array.Empty<PlayersModel>()
                };
        }

        var query = new PlayerListQuery
        {
            PlayerCodes = bankInfoPlayers,
            AgentId = agentId,
            BeginRegisteredTime = request.BeginRegisteredOn?.UtcDateTime,
            EndRegisteredTime = request.EndRegisteredOn?.UtcDateTime,
            Phone = request.Phone,
            RealName = request.RealName,
            RegisteredDomainName = request.RegisterHost,
            Status = (Tgs.PlayerService.Client.PlayerStatus?)request.Status,
            UserName = request.Username,
            VipId = request.VipId,
            ParentPlayerCode = parentPlayer?.PlayerCode
        };

        var result = playerServiceClient.QueryPlayerListAsync(
            request.BrandId,
            query,
            HttpContext.RequestAborted);

        var paging = new Paging<PlayerListItem>();

        paging.Page = request.Page;
        paging.RowCount = request.Rows;

        var totalCount = await result.CountAsync(HttpContext.RequestAborted).ConfigureAwait(false);

        result = result.ApplyPaging(paging);

        var agentNameDict = await agentQuery
            .ToDictionaryAsync(
                a => a.AgentId,
                a => a.UserName)
            .ConfigureAwait(false);

        var vipDict = await queryVipService
            .QueryAsync(new(request.BrandId), HttpContext.RequestAborted)
            .ToDictionaryAsync(
                info => info.Id,
                info => info.Level,
                HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var pagedView = await result
                .ToArrayAsync(HttpContext.RequestAborted)
                .ConfigureAwait(false);

        var parentPlayers = pagedView
            .Where(p => p.ParentPlayerCode is not null)
            .Select(p => p.ParentPlayerCode)
            .Distinct();

        var parentPlayerDict = parentPlayers.Any()
            ? await playerServiceClient
                .QueryPlayerListAsync(
                    request.BrandId,
                    new PlayerListQuery
                    {
                        PlayerCodes = parentPlayers
                    },
                    HttpContext.RequestAborted)
                .ToDictionaryAsync(
                    p => p.PlayerCode,
                    p => p.UserName,
                    HttpContext.RequestAborted)
                .ConfigureAwait(false)
            : new Dictionary<string, string>();

        return new PagedResponse<PlayersModel>
        {
            Valid = true,
            TotalCount = totalCount,
            Items = pagedView
                .Select(item => new PlayersModel
                {
                    PlayerCode = item.PlayerCode,
                    RealName = item.RealName,
                    RegisteredTime = item.RegisteredOn,
                    RegisterHost = item.RegisteredDomainName,
                    Status = (TGS.Core.Models.Enums.PlayerStatus)item.Status,
                    Username = item.UserName,
                    AgentName = item.AgentId is not null && agentNameDict.TryGetValue(item.AgentId.Value, out var agentName)
                        ? agentName
                        : null,
                    VipLevel = item.VipId is not null && vipDict.TryGetValue(item.VipId.Value, out var level)
                        ? level
                        : null,
                    ParentUsername = !string.IsNullOrEmpty(item.ParentPlayerCode)
                        && parentPlayerDict.TryGetValue(item.ParentPlayerCode, out var parentName)
                        ? parentName
                        : null
                })
                .ToArray()
        };
    }

    [HttpGet("{playerBucketId}/playerBucketHistories", Name = "GetPlayerBucketHistories")]
    [Description("Get player buckets")]
    public ValueTask<GetPlayerBucketHistoriesResponse> GetPlayerBucketHistoriesAsync(Guid playerBucketId,
        [FromServices] IGetService<GetPlayerBucketHistoriesRequest, GetPlayerBucketHistoriesResponse> service)
        => service.GetAsync(new GetPlayerBucketHistoriesRequest(playerBucketId));

    [HttpGet("{playerCode}/playerBuckets", Name = "GetPlayerBuckets")]
    [Description("Get player buckets")]
    public ValueTask<GetPlayerBucketsResponse> GetPlayerBucketsAsync(string playerCode,
        [FromServices] IGetService<GetPlayerBucketsRequest, GetPlayerBucketsResponse> service)
        => service.GetAsync(new GetPlayerBucketsRequest(playerCode));

    [HttpGet("{playerCode}/transferwallet", Name = "GetPlayerGameProviderBalances")]
    [Description("Get Player Transfer Wallet")]
    public async IAsyncEnumerable<GetPlayerGameProviderBalanceResponse> GetPlayerGameProviderBalancesAsync(
        [FromServices] IQueryService<GetPlayerGameProviderBalanceCommand, GetPlayerGameProviderBalanceResponse> service,
        string playerCode)
    {
        var datas = await service.QueryAsync(new GetPlayerGameProviderBalanceCommand(playerCode)).ToArrayAsync().ConfigureAwait(false);

        if (!datas.Any())
        {
            yield break;
        }

        foreach (var data in datas)
        {
            yield return data;
        }
    }

    [HttpGet("{playerCode}/mainwallet", Name = "GetPlayerMainWalletBalance")]
    [Description("Get Player Main wallet")]
    public ValueTask<decimal> GetPlayerMainWalletBalanceAsync(
        [FromServices] IGetService<GetPlayerMainWalletBalance, decimal> service,
        string playerCode)
        => service.GetAsync(new(playerCode));

    [HttpGet("{playerCode}/operations", Name = "GetPlayerOperations")]
    [Description("Get player operation")]
    public Task<PagedResponse<PlayerOperationModel>> GetPlayerOperationsAsync(string playerCode,
        [FromQuery] GetPlayerOperationRequest request)
    {
        request.PlayerCode = playerCode;
        return _mediator.Send(request);
    }

    [HttpGet("{playerCode}", Name = "GetSinglePlayer")]
    [Description("Get player")]
    public Task<GetPlayerResponse> GetSingleAsync(string playerCode) =>
        _mediator.Send(new GetPlayerRequest(playerCode));

    [HttpPut("{playerCode}/{type}", Name = "UpdatePlayer")]
    [Description("Update player")]
    public Task<MessageResponse> UpdateAsync(string playerCode, string type,
        [FromBody] Messages.Player.UpdatePlayerRequest request)
    {
        var updatePlayerModel = new UpdatePlayerRequest
        {
            PlayerCode = playerCode,
            Type = type,
            AgentUsername = request.AgentUsername,
            AreaCode = request.AreaCode,
            ConfirmedPassword = request.ConfirmedPassword,
            Email = request.Email,
            FacebookMessenger = request.FacebookMessenger,
            Line = request.Line,
            ParentUsername = request.ParentUsername,
            Password = request.Password,
            Phone = request.Phone,
            QQ = request.QQ,
            RealName = request.RealName,
            Remark = request.Remark,
            Status = request.Status,
            Telegram = request.Telegram,
            Viber = request.Viber,
            VipId = request.VipId,
            Wechat = request.Wechat,
            WhatsApp = request.WhatsApp,
            Birthdate = request.Birthdate
        };

        return _mediator.Send(updatePlayerModel);
    }

    private static string GetRemark(TransactionType? transactionType, string description)
    {
        switch (transactionType)
        {
            case TransactionType.CompanyAdjust:
            case TransactionType.AgentAdjust:
            case TransactionType.SystemAdjust:
            case TransactionType.TransferAdjust:
            case TransactionType.BonusAdjust:
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


            case TransactionType.GameProviderFundOut:
            case TransactionType.GameProviderFundIn:
                return description;

            default:
                return string.Empty;
        }
    }
}
