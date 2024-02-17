using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Paging;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;
using Tgs.VipLevelService.Clients;
using Tgs.VipLevelService.Common.ViewModels;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Apis.BackendOffice.Controllers.Messages.Promotion;
using TGS.Apis.BackendOffice.Models;
using TGS.Apis.BackendOffice.Resources;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Application.Queries.PlayerHandlers;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using TGS.Services.BackOffice.ApplicationServices.Players;
using TGS.Services.BackOffice.ApplicationServices.Wallets;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public partial class PlayerController(IMediator mediator, IPromotionClient client) : BaseContextController
{
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

        var response = await client.GetJoinedPromotionsAsync(promotionRequest);

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
        [FromServices] IVipLevelServiceClient vipLevelServiceClient,
        [FromServices] IQueryService<AllAgentsByBrandQuery, AgentInfo> queryAgentsService,
        
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
                Items = []
            };

        var bankInfoPlayers = Array.Empty<string>();

        if (string.IsNullOrWhiteSpace(request.PlayerCode) is false)
            bankInfoPlayers = [.. bankInfoPlayers, request.PlayerCode];

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
                    Items = []
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
            : [];

        List<PlayersModel> players = [];

        foreach (var item in pagedView)
        {
            var playerVipLevel = await vipLevelServiceClient.PlayerVipLevelGetAsync(
                playerCode: item.PlayerCode,
                cancellationToken: HttpContext.RequestAborted)
                .ConfigureAwait(false);

            players.Add(new PlayersModel
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
                VipLevel = playerVipLevel.VipLevel,
                ParentUsername = !string.IsNullOrEmpty(item.ParentPlayerCode)
                        && parentPlayerDict.TryGetValue(item.ParentPlayerCode, out var parentName)
                        ? parentName
                        : null
            });
        }

        return new PagedResponse<PlayersModel>
        {
            Valid = true,
            TotalCount = totalCount,
            Items = [.. players]
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

        if (datas.Length == 0)
        {
            yield break;
        }

        foreach (var data in datas)
        {
            yield return data;
        }
    }

    [HttpGet("histories/{playerCode}", Name = "GetPlayerHistories")]
    public async ValueTask<PagedResponse<PlayerHistoryViewModel>> GetPlayerHistoriesAsync(
        [FromServices] IQueryService<PlayerHistoriesQuery, PlayerHistoryInfo> queryService,
        [FromServices] IGetService<PlayerHistoriesQuery, int> countService,
        [FromServices] ITgsQueries<Core.DataServices.Tgs.Models.User> userQueries,
        string playerCode,
        [FromQuery] PlayerHistoriesQueryViewModel queryViewModel,
        [FromQuery(Name = "page")] int page = 1,
        [FromQuery(Name = "rows")] int rows = 10)
    {
        var query = new PlayerHistoriesQuery(
            playerCode,
            queryViewModel.From,
            queryViewModel.To,
            (page - 1) * rows);

        var totalCount = await countService
            .GetAsync(query, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var infos = await queryService
            .QueryAsync(query, HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted).ConfigureAwait(false);

        var userIds = infos
            .Where(x => !string.IsNullOrWhiteSpace(x.Modifier) && Guid.TryParse(x.Modifier, out var _) == true)
            .Select(x => x.Modifier)
            .Select(x => new Guid(x));

        var userQuery = userQueries.GetQueries().Where(u => userIds.Contains(u.Id)).ToArray();

        var user = userQuery.Length != 0
            ? userQuery.ToDictionary(x => x.Id, x => x.Username)
            : [];

        return new PagedResponse<PlayerHistoryViewModel>
        {
            Valid = true,
            TotalCount = totalCount,
            Items = infos.Select(x => new PlayerHistoryViewModel
            {
                Action = x.Action,
                CreatedOn = x.CreatedOn,
                EventId = x.EventId,
                Modifier = TryParseUsername(user, x.Modifier)
            }).ToArray(),
        };
    }

    [HttpGet("history/{eventId}", Name = "GetPlayerHistory")]
    public async ValueTask<PlayerHistoryContentViewModel> GetPlayerHistoryAsync(
        [FromServices] IGetService<PlayerHistoryQuery, PlayerHistoryEventInfo> getService,
        [FromServices] ITgsQueries<Core.DataServices.Tgs.Models.User> userQueries,
        Guid eventId)
    {
        var info = await getService
            .GetAsync(new PlayerHistoryQuery(eventId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        _ = Guid.TryParse(info.Modifier, out var userId);

        var user = userQueries.GetQueries()
            .Where(u => u.Id == userId)
            .FirstOrDefault();

        return new PlayerHistoryContentViewModel
        {
            Action = info.Action,
            ChangeSets = info.ChangeSets,
            CreatedOn = info.CreatedOn,
            ModifyUsername = user == null
                ? info.Modifier
                : user.Username
        };
    }

    [HttpGet("{playerCode}/operations", Name = "GetPlayerOperations")]
    [Description("Get player operation")]
    public Task<PagedResponse<PlayerOperationModel>> GetPlayerOperationsAsync(string playerCode,
        [FromQuery] GetPlayerOperationRequest request)
    {
        request.PlayerCode = playerCode;
        return mediator.Send(request);
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
    public async Task<PagedResponse<PlayerVipAdjustViewModel>> GetVipAdjustInfosAsync(
        string playerCode,
        [FromServices] IGetService<GetPlayerVipAdjustQuery, PagingResponseViewModel<PlayerVipLevelLogInfoViewModel>> service,
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

        return new PagedResponse<PlayerVipAdjustViewModel>
        {
            Valid = true,
            Message = string.Empty,
            TotalCount = result.TotalCount,
            Items = result.Items.Select(logs => new PlayerVipAdjustViewModel(
                Type: logs.Type,
                OldLevel: logs.BeforeLevel,
                OldAutoState: logs.BeforeAutoState,
                NewLevel: logs.AfterLevel,
                NewAutoState: logs.AfterAutoState,
                CreatedOn: logs.CreatedOn.DateTime,
                CreatedUsername: logs.CreatedUsername,
                Remark: logs.Remark)
            ).ToArray()
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

    [HttpPatch("agent/{playerCode}", Name = "SetPlayerAgent")]
    [Description("Update player's agent")]
    public async ValueTask<MessageResponse> SetPlayerAgentAsync(
        [FromServices] IGetService<SetPlayerAgentCommand, SetPlayerAgentResult> getService,
        string playerCode,
        [FromBody] UpdatePlayerAgentViewModel request)
    {
        var processInfo = await getService
            .GetAsync(new SetPlayerAgentCommand(playerCode, request.AgentUsername, UserId, Username), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = processInfo.Valid,
            Message = processInfo.Message,
        };
    }

    [HttpPatch("contact/{playerCode}", Name = "SetPlayerContact")]
    public async ValueTask<MessageResponse> SetPlayerContactAsync(
        [FromServices] IGetService<SetPlayerContactCommand, SetPlayerContactInfo> setPlayerContactService,
        string playerCode,
        [FromBody] SetPlayerContactRequest request)
    {
        var processInfo = await setPlayerContactService
            .GetAsync(new SetPlayerContactCommand(playerCode, UserId, Username, request), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = processInfo.Valid,
            Message = processInfo.Message
        };
    }

    [HttpPatch("info/{playerCode}", Name = "SetPlayerInfo")]
    [Description("Update player info (realname or birthdate or remark or vip level or idNumber)")]
    public async ValueTask<MessageResponse> SetPlayerInfoAsync(
        [FromServices] IGetService<SetPlayerInfoCommand, SetPlayerInfoResult> getService,
        string playerCode,
        [FromBody] SetPlayerInfoRequest request)
    {
        var processInfo = await getService
            .GetAsync(new SetPlayerInfoCommand(playerCode, UserId, Username, request), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = processInfo.Valid,
            Message = processInfo.Message,
        };
    }

    [HttpPatch("parent-player/{playerCode}", Name = "SetPlayerParentPlayer")]
    [Description("Update player's parent player")]
    public async ValueTask<MessageResponse> SetPlayerParentPlayerAsync(
        [FromServices] IGetService<SetPlayerParentPlayerCommand, SetPlayerParentPlayerInfo> getService,
        string playerCode,
        [FromBody] UpdatePlayerParentPlayerViewModel request)
    {
        var proccessInfo = await getService
            .GetAsync(new SetPlayerParentPlayerCommand(playerCode, request.ParentPlayerUsername, Username), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = proccessInfo.Valid,
            Message = proccessInfo.Message,
        };
    }

    [HttpPatch("password/{playerCode}/{password}")]
    [Description("Update player login password")]
    public async ValueTask<MessageResponse> SetPlayerPasswordAsync(
        [FromServices] IGetService<SetPlayerPasswordCommand, SetPlayerPasswordInfo> setPlayerPasswordService,
        string playerCode,
        string password)
    {
        var command = new SetPlayerPasswordCommand(playerCode, password, UserId, Username);

        var processInfo = await setPlayerPasswordService
            .GetAsync(command, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = processInfo.Valid,
            Message = processInfo.Message
        };
    }

    [HttpPatch("status/{playerCode}", Name = "SetPlayerStatus")]
    [Description("Update player status")]
    public async ValueTask<MessageResponse> SetPlayerStatusAsync(
        [FromServices] IGetService<SetPlayerStatusCommand, SetPlayerStatusResult> getService,
        string playerCode,
        [FromBody] UpdatePlayerStatusViewModel request)
    {
        var processInfo = await getService
            .GetAsync(new SetPlayerStatusCommand(playerCode, request.Status, UserId, Username), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = processInfo.Valid,
            Message = processInfo.Message,
        };
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

        return mediator.Send(updatePlayerModel);
    }

    [HttpPut("identityauth/{playerCode}/{identityAuthentication}")]
    [Description("Update player identity authentication")]
    [TgsAction("UpdatePlayerIdentityAuthentication")]
    public async Task UpdatePlayerIdentityAuthenticationAsync(
        string playerCode,
        bool identityAuthentication,
        [FromServices] ICommandService<UpdatePlayerIdentityAuthenticationCommand> updateIdentityAuthService)
    {
        var command = new UpdatePlayerIdentityAuthenticationCommand(playerCode, identityAuthentication, UserId);

        await updateIdentityAuthService
            .ExecuteAsync(command)
            .ConfigureAwait(false);
    }

    [HttpPost("upgrade/{playerCode}/agent")]
    [Description("Upgrade player to agent")]
    public Task UpgradePlayerToAgentAsync(string playerCode)
        => mediator.Send(new UpgradePlayerToAgentRequest(playerCode, Username));

    private static string TryParseUsername(Dictionary<Guid, string> UserDicts, string Modifier)
    {
        if (Guid.TryParse(Modifier, out var userId) && UserDicts.Count != 0)
        {
            _ = UserDicts.TryGetValue(userId, out var userName);

            return userName;
        }

        return Modifier;
    }
}
