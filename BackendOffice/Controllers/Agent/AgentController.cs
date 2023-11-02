using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Agent;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Agent;
using TGS.Services.BackOffice.Application.Models.AgentAdjust;
using TGS.Services.BackOffice.Application.Models.ReportAgent;
using TGS.Services.BackOffice.ApplicationServices.Agents;

namespace TGS.Apis.BackendOffice.Controllers.Agent;

[Route("api/[controller]")]
[ApiController]
public partial class AgentController : BaseContextController
{
    private readonly IReportClient _client;
    private readonly IMediator _mediator;

    public AgentController(IMediator mediator, IReportClient client)
    {
        _mediator = mediator;
        _client = client;
    }

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
    public Task<MessageResponse> CreateAgentAsync(CreateAgentRequest request) => _mediator.Send(request);

    [HttpPost("{id:guid}/adjust/increase")]
    [Description("Create an agent adjustment to increase amount")]
    public Task<MessageResponse> CreateAgentIncreaseAmountAsync(Guid id, CreateAgentIncreaseAmountRequest request)
    {
        request.AgentId = id;
        return _mediator.Send(request);
    }

    [HttpPost("{id:guid}/adjust/reduce")]
    [Description("Create an agent adjustment to reduce amount")]
    public async Task<MessageResponse> CreateAgentReduceAmountAsync(
        Guid id,
        CreateAgentReduceAmountRequest request,
        [FromServices] IGetService<AgentReduceAmountQuery, AgentReduceAmountResult> getService)
    {
        var result = await getService
            .GetAsync(new AgentReduceAmountQuery(
                AgentId: id,
                request.Amount,
                request.Remark,
                request.TargetName,
                request.TransactionType,
                request.AccountType,
                Username,
                UserId))
            .ConfigureAwait(false);

        return result.Valid
            ? CreateValidResponse<MessageResponse>()
            : CreateInvalidResponse<MessageResponse>(result.Message);
    }

    [HttpGet("accountChanges")]
    [Description("Get agent account changes")]
    public async ValueTask<PagedResponse<AgentAccountChangeReportModel>> GetAgentAccountChangesAsync(
        [FromQuery] GetAgentAccountChangesReportRequest request)
    {
        var reportRequest = new GetAgentAccountChangesRequest
        {
            BrandId = request.BrandId,
            AgentId = request.AgentId,
            TransactionType = (short?)request.TransactionType,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };

        var response = await _client.GetAgentAccountChangesAsync(reportRequest);

        return new()
        {
            Valid = response.Valid,
            Message = response.Message,
            TotalCount = response.TotalCount,
            Items = response.Items?.Select(x => new AgentAccountChangeReportModel
            {
                TransactionId = x.TransactionId,
                Amount = x.Amount,
                Balance = x.Balance,
                TransactionType = (TransactionType)x.TransactionType,
                RecordedOn = x.RecordedOn
            }).ToArray()
        };
    }

    [HttpGet("{id:guid}")]
    [Description("Get an agent")]
    public Task<GetAgentResponse> GetAgentAsync(Guid id) => _mediator.Send(new GetAgentRequest(id));

    [HttpGet("dividendPercentage/{brandId:guid}/{username}")]
    [Description("Get an agent dividend percentage by username")]
    public Task<GetAgentDividendSettingResponse> GetAgentDividendPercentageAsync(Guid brandId, string username)
        => _mediator.Send(new GetAgentDividendSettingRequest(brandId, username));

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
        _mediator.Send(request);

    [HttpPut("{id:guid}")]
    [Description("Update a agent")]
    public Task<MessageResponse> UpdateAgentAsync(Guid id, UpdateAgentRequest request)
    {
        request.Id = id;
        return _mediator.Send(request);
    }

    [HttpPatch("pay-password", Name = "UpdateAgentPayPassword")]
    [Description("Update Agent Pay Password")]
    public async Task<MessageResponse> UpdateAgentPayPasswordAsync(
        [FromServices] ICommandService<UpdateAgentPayPasswordCommand> commandService,
        UpdateAgentPayPasswordRequest request)
    {
        await commandService
            .ExecuteAsync(new UpdateAgentPayPasswordCommand(request.AgentId, request.PayPassword, Username))
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

        var user = userQuery.Any()
            ? userQuery.ToDictionary(x => x.Id, x => x.Username)
            : new Dictionary<Guid, string>();

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
        if (Guid.TryParse(Modifier, out var userId) && UserDicts.Any())
        {
            _ = UserDicts.TryGetValue(userId, out var userName);

            return userName;
        }

        return Modifier;
    }
}
