using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Agent;
using Tgs.ServiceUtility;
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

    [HttpPost]
    [Description("Create a agent")]
    public Task<MessageResponse> CreateAgentAsync(CreateAgentRequest request) => _mediator.Send(request);

    [HttpPost("{id}/adjust/increase")]
    [Description("Create an agent adjustment to increase amount")]
    public Task<MessageResponse> CreateAgentIncreaseAmountAsync(Guid id, CreateAgentIncreaseAmountRequest request)
    {
        request.AgentId = id;
        return _mediator.Send(request);
    }

    [HttpPost("{id}/adjust/reduce")]
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

    [HttpGet("{id}")]
    [Description("Get an agent")]
    public Task<GetAgentResponse> GetAgentAsync(Guid id) => _mediator.Send(new GetAgentRequest(id));

    [HttpGet("dividendPercentage/{brandId}/{username}")]
    [Description("Get an agent dividend percentage by username")]
    public Task<GetAgentDividendSettingResponse> GetAgentDividendPercentageAsync(Guid brandId, string username)
        => _mediator.Send(new GetAgentDividendSettingRequest(brandId, username));

    [HttpGet("{id}/operations")]
    [Description("Get agent operations")]
    public Task<PagedResponse<AgentOperationModel>> GetAgentOperationsAsync(Guid id) =>
        _mediator.Send(new GetAgentOperationsRequest(id));

    [HttpGet]
    [Description("Get agents")]
    public Task<PagedResponse<AgentModel>> GetAgentsAsync([FromQuery] GetAgentsRequest request) =>
        _mediator.Send(request);

    [HttpPut("{id}")]
    [Description("Update a agent")]
    public Task<MessageResponse> UpdateAgentAsync(Guid id, UpdateAgentRequest request)
    {
        request.Id = id;
        return _mediator.Send(request);
    }
}
