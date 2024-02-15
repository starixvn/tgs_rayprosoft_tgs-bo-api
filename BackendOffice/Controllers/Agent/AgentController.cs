using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.Agent;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using TGS.Services.BackOffice.Domain;
using CreateAgentRequest = TGS.Services.BackOffice.Application.Models.Agent.CreateAgentRequest;

namespace TGS.Apis.BackendOffice.Controllers.Agent;

[Route("api/[controller]")]
[ApiController]
public partial class AgentController(IMediator mediator) : BaseContextController
{
    [HttpPut("{id:guid}/clear-negative-commission")]
    [Description("Clear negative commission")]
    public async Task<MessageResponse> ClearUnDistributedCommissionAsync(
        [FromServices] IAgentClient agentService,
        Guid id)
    {
        await agentService.ClearAgentNegativeCommissionAsync(new(id, Guid.Parse(UserId)));

        return CreateValidResponse<MessageResponse>();
    }

    [HttpPost]
    [Description("Create a agent")]
    public Task<MessageResponse> CreateAgentAsync(CreateAgentRequest request) => mediator.Send(request);

    [HttpPost("{id:guid}/adjust/increase")]
    [Description("Create an agent adjustment to increase amount")]
    public async Task<MessageResponse> CreateAgentIncreaseAmountAsync(
        [FromServices] IGetService<AgentIncreaseAmountCommand, AgentAdjustAmountResult> adjustService,
        Guid id,
        CreateAgentIncreaseAmountViewModel viewModel)
    {
        if (!Enum.IsDefined(viewModel.TransactionType) ||
            !Enum.IsDefined((AgentAdjustmentTransactionType)viewModel.TransactionType))
            return CreateInvalidResponse<MessageResponse>("is not define type");

        var command = new AgentIncreaseAmountCommand(
            id,
            viewModel.Amount,
            viewModel.Remark,
            (AgentAdjustmentTransactionType)viewModel.TransactionType,
            Guid.Parse(UserId),
            Username);

        var result = await adjustService.GetAsync(
            command,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return result.Valid
            ? CreateValidResponse<MessageResponse>()
            : CreateInvalidResponse<MessageResponse>(result.Message);
    }

    [HttpPost("{walletId:guid}/adjust/reduce")]
    [Description("Create an agent adjustment to reduce amount")]
    public async Task<MessageResponse> CreateAgentReduceAmountAsync(
        [FromServices] IGetService<AgentReduceAmountCommand, AgentAdjustAmountResult> getService,
        Guid walletId,
        CreateAgentReduceAmountViewModel viewModel)
    {
        if (!Enum.IsDefined(viewModel.TransactionType)
            || !Enum.IsDefined((AgentAdjustmentTransactionType)viewModel.TransactionType))
            return CreateInvalidResponse<MessageResponse>("is not define type");

        var result = await getService.GetAsync(
            new AgentReduceAmountCommand(
                walletId,
                viewModel.Amount,
                viewModel.Remark,
                viewModel.TargetName,
                (AgentAdjustmentTransactionType)viewModel.TransactionType,
                viewModel.AccountType,
                Username,
                Guid.Parse(UserId)),
            HttpContext.RequestAborted).ConfigureAwait(false);

        return result.Valid
            ? CreateValidResponse<MessageResponse>()
            : CreateInvalidResponse<MessageResponse>(result.Message);
    }

    [HttpGet("{id:guid}")]
    [Description("Get an agent")]
    public Task<GetAgentResponse> GetAgentAsync(Guid id) => mediator.Send(new GetAgentRequest(id));

    [HttpGet("dividendPercentage/{brandId:guid}/{username}")]
    [Description("Get an agent dividend percentage by username")]
    public Task<GetAgentDividendSettingResponse> GetAgentDividendPercentageAsync(Guid brandId, string username)
        => mediator.Send(new GetAgentDividendSettingRequest(brandId, username));

    [HttpGet("{id:guid}/operations")]
    [Description("Get agent operations")]
    public async Task<PagedResponse<AgentOperationModel>> GetAgentOperationsAsync(
        Guid id,
        [FromServices] IGetService<GetAgentOperationQuery, PagingResponse<AgentAuditLogInfo>> getService,
        [FromQuery] GetAgentOperationsRequest request,
        [FromQuery] PageableRequest pageableRequest)
    {
        var result = await getService
            .GetAsync(new GetAgentOperationQuery(request.BrandId, id, request.From, request.To, pageableRequest), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new PagedResponse<AgentOperationModel>
        {
            Valid = true,
            TotalCount = result.TotalCount,
            Items = result.Items.Select(x => new AgentOperationModel
            {
                Id = x.Id,
                CreatedByUsername = x.CreatedByUsername,
                CreatedOn = x.CreatedOn,
                Operation = x.Operation
            }).ToArray()
        };
    }

    [HttpGet]
    [Description("Get agents")]
    public Task<PagedResponse<AgentModel>> GetAgentsAsync([FromQuery] GetAgentsRequest request) =>
        mediator.Send(request);

    [HttpPut("{id:guid}")]
    [Description("Update a agent")]
    public Task<MessageResponse> UpdateAgentAsync(Guid id, UpdateAgentRequest request)
    {
        request.Id = id;
        return mediator.Send(request);
    }

    [HttpPatch("pay-password", Name = "UpdateAgentPayPassword")]
    [Description("Update Agent Pay Password")]
    public async Task<MessageResponse> UpdateAgentPayPasswordAsync(
        [FromServices] ICommandService<UpdateAgentPayPasswordCommand> commandService,
        UpdateAgentPayPasswordRequest request)
    {
        await commandService
            .ExecuteAsync(new UpdateAgentPayPasswordCommand(request.AgentId, request.PayPassword, Username, UserId))
            .ConfigureAwait(false);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpPatch("contact/{agentId}", Name = "SetAgentContact")]
    public async ValueTask<MessageResponse> SetAgentContactAsync(
        [FromServices] IGetService<SetAgentContactCommand, SetAgentContactResult> setService,
        Guid agentId,
        [FromBody] ViewModels.SetAgentContactRequest request)
    {
        var processInfo = await setService
            .GetAsync(new SetAgentContactCommand(agentId, request, UserId, Username), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = processInfo.Valid,
            Message = processInfo.Message
        };
    }

    [HttpPatch("password/{agentId}/{password}", Name = "SetAgentPassword")]
    [Description("Update agent login password")]
    public async ValueTask<MessageResponse> SetAgentPasswordAsync(
        [FromServices] IGetService<SetAgentPasswordCommand, SetAgentPasswordResult> setPlayerPasswordService,
        Guid agentId,
        string password)
    {
        var command = new SetAgentPasswordCommand(agentId, password, UserId, Username);

        var processInfo = await setPlayerPasswordService
            .GetAsync(command, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = processInfo.Valid,
            Message = processInfo.Message
        };
    }

    [HttpPatch("status/{agentId}", Name = "SetAgentStatus")]
    [Description("Update agent status")]
    public async ValueTask<MessageResponse> SetAgentStatusAsync(
        [FromServices] IGetService<SetAgentStatusCommand, SetAgentStatusResult> getService,
        Guid agentId,
        [FromBody] SetAgentStatusViewModel request)
    {
        var processInfo = await getService
            .GetAsync(new SetAgentStatusCommand(agentId, request.Status, UserId, Username), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = processInfo.Valid,
            Message = processInfo.Message,
        };
    }

    [HttpPatch("info/{agentId}", Name = "SetAgentInfo")]
    [Description("Update agent info (realname or remark)")]
    public async ValueTask<MessageResponse> SetAgentInfoAsync(
        [FromServices] IGetService<SetAgentInfoCommand, SetAgentInfoResult> getService,
        Guid agentId,
        [FromBody] SetAgentInfoViewModel request)
    {
        var processInfo = await getService
            .GetAsync(new SetAgentInfoCommand(agentId, request, UserId, Username), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = processInfo.Valid,
            Message = processInfo.Message,
        };
    }

    [HttpPatch("one-layer/{agentId}", Name = "SetOneLayerAgent")]
    [Description("Update one layer agent's parent or dividend percentage or type")]
    public async ValueTask<MessageResponse> SetOneLayerAgentAsync(
        [FromServices] IGetService<SetAgentOneLayerCommand, SetAgentOneLayerResult> getService,
        Guid agentId,
        [FromBody] SetOneLayerAgentInfoViewModel request)
    {
        var processInfo = await getService
            .GetAsync(new SetAgentOneLayerCommand(agentId, request.ParentUsername, request.DividendPercentage, request.Type, Username))
            .ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = processInfo.Valid,
            Message = processInfo.Message,
        };
    }

    [HttpPatch("multiple-layer/{agentId}", Name = "SetMultipleLayerAgent")]
    [Description("Update multiple layer agent's parent or dividend percentage or type")]
    public async ValueTask<MessageResponse> SetMultipleLayerAgentAsync(
        [FromServices] IGetService<SetAgentMultipleLayerCommand, SetAgentMultipleLayerResult> getService,
        Guid agentId,
        [FromBody] SetMultipleLayerAgentInfoViewModel request)
    {
        var processInfo = await getService
            .GetAsync(new SetAgentMultipleLayerCommand(agentId, request.ParentUsername, request.AgentDividendConditions, request.Type, Username))
            .ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = processInfo.Valid,
            Message = processInfo.Message,
        };
    }

    [HttpGet("histories/{agentId}", Name = "GetAgentHistories")]
    public async ValueTask<PagedResponse<AgentHistoryViewModel>> GetAgentHistoriesAsync(
        [FromServices] IQueryService<AgentHistoriesQuery, AgentHistoryInfo> queryService,
        [FromServices] IGetService<AgentHistoriesQuery, int> countService,
        [FromServices] ITgsQueries<Core.DataServices.Tgs.Models.User> userQueries,
        Guid agentId,
        [FromQuery] AgentHistoriesQueryViewModel queryViewModel,
        [FromQuery(Name = "page")] int page = 1,
        [FromQuery(Name = "rows")] int rows = 10)
    {
        var query = new AgentHistoriesQuery(
            agentId,
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

        return new PagedResponse<AgentHistoryViewModel>
        {
            Valid = true,
            TotalCount = totalCount,
            Items = infos.Select(x => new AgentHistoryViewModel
            {
                Action = x.Action,
                CreatedOn = x.CreatedOn,
                EventId = x.EventId,
                Modifier = TryParseUsername(user, x.Modifier)
            }).ToArray(),
        };
    }

    [HttpGet("history/{eventId}", Name = "GetAgentHistory")]
    public async ValueTask<AgentHistoryContentViewModel> GetAgentHistoryAsync(
        [FromServices] IGetService<AgentHistoryQuery, AgentHistoryEventInfo> getService,
        [FromServices] ITgsQueries<Core.DataServices.Tgs.Models.User> userQueries,
        Guid eventId)
    {
        var info = await getService
            .GetAsync(new AgentHistoryQuery(eventId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        _ = Guid.TryParse(info.Modifier, out var userId);

        var user = userQueries.GetQueries()
            .Where(u => u.Id == userId)
            .FirstOrDefault();

        return new AgentHistoryContentViewModel
        {
            Action = info.Action,
            ChangeSets = info.ChangeSets,
            CreatedOn = info.CreatedOn,
            ModifyUsername = user == null
                ? info.Modifier
                : user.Username
        };
    }

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
