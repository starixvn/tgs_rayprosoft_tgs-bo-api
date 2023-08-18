using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.CashQueryService.Client;
using Tgs.CashService.Client;
using Tgs.Paging;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;
using Tgs.VipLevelService.Clients;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Apis.BackendOffice.Controllers.Messages.Promotion;
using TGS.Apis.BackendOffice.Models;
using TGS.Apis.BackendOffice.Resources;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Application.Models.ReportPlayer;
using TGS.Services.BackOffice.Application.Queries.PlayerHandlers;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using TGS.Services.BackOffice.Domain;
using BoEnums = TGS.Services.BackOffice.Domain.Enums;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public partial class PlayerController : BaseContextController
{
    private readonly IPromotionClient _client;
    private readonly IMediator _mediator;

    public PlayerController(IMediator mediator, IPromotionClient client)
    {
        _mediator = mediator;
        _client = client;
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

    [HttpPost("{playerCode}/fundin", Name = "FundInPlayerGameProvider")]
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
        [FromQuery] GetPlayerAccountChangesReportRequest request,
        [FromServices] IPlayerServiceClient playerServiceClient,
        [FromServices] IGetService<AccountChangesQuery, Tgs.CashService.Client.PagingResponse<TransactionLog>> getService,
        [FromServices] ICashQueryClient cashQueryClient)
    {
        var playerProps = await playerServiceClient
            .GetPlayerPropertiesAsync(request.PlayerCode, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var playerWalletId = Guid.Parse(playerProps["DefaultWalletId"]!);

        var queryTransactionTypes = Array.Empty<int>();

        if (request.AccountChangeType is null)
        {
            queryTransactionTypes = Enum
                .GetValues(typeof(BoEnums.TransactionType))
                .Cast<BoEnums.TransactionType>()
                .Where(x => x is not BoEnums.TransactionType.WithdrawCommit
                    and not BoEnums.TransactionType.TransferCommit)
                .Select(x => (int)x)
                .ToArray();
        }
        else
        {
            switch (request.AccountChangeType.Value)
            {
                case BoEnums.TransactionType.Deposit:
                    queryTransactionTypes = new int[] {
                        (int)BoEnums.TransactionType.Deposit,
                        (int)BoEnums.TransactionType.Deposited
                    };
                    break;

                case BoEnums.TransactionType.Withdrawal:
                    queryTransactionTypes = new int[] {
                        (int)BoEnums.TransactionType.Withdrawal,
                        (int)BoEnums.TransactionType.WithdrawLock
                    };
                    break;

                case BoEnums.TransactionType.CancelWithdrawal:
                    queryTransactionTypes = new int[] {
                        (int)BoEnums.TransactionType.CancelWithdrawal,
                        (int)BoEnums.TransactionType.WithdrawRollback
                    };
                    break;

                default:
                    queryTransactionTypes = new int[] {
                        (int)request.AccountChangeType.Value
                    };
                    break;
            }
        }

        var query = new AccountChangesQuery(
            playerWalletId,
            request.From.DateTime,
            request.To.DateTime,
            queryTransactionTypes,
            new PagingConditions(request.Page, request.Rows));

        var result = await getService
            .GetAsync(query)
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

    [HttpGet("accountChanges/totalAmount", Name = "GetAccountChangesTotalAmount")]
    [Description("Get player account changes Total Amount")]
    public async Task<PlayerAccountChangesTotalModel> GetAccountChangesTotalAmountAsync(
        [FromServices] IPlayerServiceClient playerServiceClient,
        [FromServices] ICashServiceClient cashSerivceClient,
        [FromQuery] GetPlayerAccountChangesReportTotalAmountRequest request)
    {
        var playerProps = await playerServiceClient
            .GetPlayerPropertiesAsync(request.PlayerCode, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var playerWalletId = Guid.Parse(playerProps["DefaultWalletId"]!);

        var queryTransactionTypes = Array.Empty<int>();

        if (request.AccountChangeType is null)
        {
            queryTransactionTypes = Enum
                .GetValues(typeof(BoEnums.TransactionType))
                .Cast<BoEnums.TransactionType>()
                .Where(x => x is not BoEnums.TransactionType.WithdrawCommit
                    and not BoEnums.TransactionType.TransferCommit)
                .Select(x => (int)x)
                .ToArray();
        }
        else
        {
            switch (request.AccountChangeType.Value)
            {
                case BoEnums.TransactionType.Deposit:
                    queryTransactionTypes = new int[] {
                        (int)BoEnums.TransactionType.Deposit,
                        (int)BoEnums.TransactionType.Deposited
                    };
                    break;

                case BoEnums.TransactionType.Withdrawal:
                    queryTransactionTypes = new int[] {
                        (int)BoEnums.TransactionType.Withdrawal,
                        (int)BoEnums.TransactionType.WithdrawLock
                    };
                    break;

                case BoEnums.TransactionType.CancelWithdrawal:
                    queryTransactionTypes = new int[] {
                        (int)BoEnums.TransactionType.CancelWithdrawal,
                        (int)BoEnums.TransactionType.WithdrawRollback
                    };
                    break;

                default:
                    queryTransactionTypes = new int[] {
                        (int)request.AccountChangeType.Value
                    };
                    break;
            }
        }

        var response = await cashSerivceClient
            .QueryTransactionTotalAmountAsync(playerWalletId, request.From.DateTime, request.To.DateTime, queryTransactionTypes, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new PlayerAccountChangesTotalModel { Amount = response ?? 0M };
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

        var bankInfoPlayers = Array.Empty<string>();

        if (string.IsNullOrWhiteSpace(request.PlayerCode) is false)
            bankInfoPlayers = bankInfoPlayers.Append(request.PlayerCode).ToArray();

        if (string.IsNullOrWhiteSpace(request.PlayerCode) is true
            && !string.IsNullOrWhiteSpace(request.BankInfoTagType)
            && !string.IsNullOrWhiteSpace(request.BankInfoTag))
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

        var paging = new Paging<PlayerListItem>
        {
            Page = request.Page,
            RowCount = request.Rows
        };

        var totalCount = await result.CountAsync(
            HttpContext.RequestAborted).ConfigureAwait(false);

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
            ? await playerServiceClient.QueryPlayerInfoListAsync(
                request.BrandId,
                parentPlayers)
                .ToDictionaryAsync(
                    info => info.PlayerCode,
                    info => info.UserName,
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
                    Status = (Core.Models.Enums.PlayerStatus)item.Status,
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

    [HttpGet("{playerCode}/transferwallet", Name = "GetPlayerGameProviderBalances")]
    [Description("Get Player Transfer Wallet")]
    public async IAsyncEnumerable<GetPlayerGameProviderBalanceResponse> GetPlayerGameProviderBalancesAsync(
        [FromServices] IGetService<GetPlayerBalanceQuery, decimal> getPlayerBalanceService,
        [FromServices] IQueryService<GetPlayerGameProviderBalanceQuery, GetPlayerGameProviderBalanceResponse> queryGameBalanceService,
        string playerCode)
    {
        async ValueTask<GetPlayerGameProviderBalanceResponse> GetPlayerBalanceAsync(CancellationToken cancellationToken)
        {
            try
            {
                var walletBalance = await getPlayerBalanceService.GetAsync(
                    new(playerCode),
                    cancellationToken).ConfigureAwait(false);

                return new GetPlayerGameProviderBalanceResponse(
                    "DEFAULT",
                    "DEFAULT",
                    true,
                    false,
                    walletBalance);
            }
            catch
            {
                return new GetPlayerGameProviderBalanceResponse(
                    GameProviderCode: "DEFAULT",
                    GameProviderName: "DEFAULT",
                    IsMainWallet: true,
                    IsMaintain: true,
                    Balance: null);
            }
        }

        yield return await GetPlayerBalanceAsync(HttpContext.RequestAborted).ConfigureAwait(false);

        var datas = await queryGameBalanceService.QueryAsync(new GetPlayerGameProviderBalanceQuery(playerCode))
            .ToArrayAsync()
            .ConfigureAwait(false);

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
    public async Task<GetPlayerResponse> GetSingleAsync(
        [FromServices] IGetService<GetSinglePlayerQuery, PlayerModel> getService,
        string playerCode)
    {
        var query = new GetSinglePlayerQuery(playerCode);

        var player = await getService.GetAsync(
            query,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return player is null
            ? new GetPlayerResponse
            {
                Valid = false,
                Message = Player.NotFound
            }
            : new GetPlayerResponse
            {
                Valid = true,
                Player = player
            };
    }

    [HttpGet("vipAdjustInfos/{playerCode}", Name = "GetVipAdjustInfos")]
    [Description("Get player vip adjust infos")]
    public async Task<PagedResponse<PlayerVipAdjustInfo>> GetVipAdjustInfosAsync(
        string playerCode,
        [FromServices] IGetService<GetPlayerVipAdjustQuery, Tgs.VipLevelService.Clients.PagingResponse<PlayerVipAdjustInfo>> service,
        [FromQuery] Messages.Player.GetVipAdjustInfosRequest request,
        [FromQuery] PageableRequest paging)
    {
        var query = new GetPlayerVipAdjustQuery(
            playerCode,
            request.BrandId,
            request.From,
            request.To,
            paging);

        var result = await service.GetAsync(query)
            .ConfigureAwait(false);

        return new PagedResponse<PlayerVipAdjustInfo>
        {
            Valid = true,
            Message = string.Empty,
            TotalCount = result.TotalCount,
            Items = result.Items.ToArray(),
        };
    }

    [HttpGet("{brandId:Guid}/{userNamePrefix}", Name = "ListPlayerCodesByUserNamePrefix")]
    public IAsyncEnumerable<JsonDocument> ListPlayerCodesByUserNamePrefixAsync(
        [FromServices] IQueryService<ListPlayerCodesByUserNamePrefixQuery, KeyValuePair<string, string>> queryService,
        Guid brandId,
        string userNamePrefix)
    {
        var query = new ListPlayerCodesByUserNamePrefixQuery(brandId, userNamePrefix);

        return queryService.QueryAsync(query, HttpContext.RequestAborted)
            .Take(10)
            .Select(info => JsonDocument.Parse($$"""{"{{info.Value}}":"{{info.Key}}"}"""));
    }

    [HttpPut("{playerCode}/{type}", Name = "UpdatePlayer")]
    [Description("Update player")]
    [TgsAction("UpdatePlayer")]
    public Task<MessageResponse> UpdateAsync(
        string playerCode,
        string type,
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

    [HttpPut("identityauth/{playerCode}/{identityAuthentication}")]
    [Description("Update player identity authentication")]
    [TgsAction("UpdatePlayerIdentityAuthentication")]
    public async Task UpdatePlayerIdentityAuthenticationAsync(
        string playerCode,
        bool identityAuthentication,
        [FromServices] ICommandService<UpdatePlayerIdentityAuthenticationCommand> updateIdentityAuthService)
    {
        var command = new UpdatePlayerIdentityAuthenticationCommand(playerCode, identityAuthentication);

        await updateIdentityAuthService
            .ExecuteAsync(command)
            .ConfigureAwait(false);
    }

    private static string GetRemark(BoEnums.TransactionType? transactionType, string description)
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
                return description;

            default:
                return string.Empty;
        }
    }
}
