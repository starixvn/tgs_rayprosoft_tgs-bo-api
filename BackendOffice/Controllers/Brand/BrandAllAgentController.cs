using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.BrandAllAgent;

namespace TGS.Apis.BackendOffice.Controllers.Brand;

[Route("api/[controller]")]
[ApiController]
public class BrandAllAgentController : BaseContextController
{
    private readonly IMediator _mediator;

    public BrandAllAgentController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id}")]
    [Description("Get the brand's all agent settings")]
    public Task<GetBrandAllAgentResponse> GetBrandAllAgentAsync(Guid id)
        => _mediator.Send(new GetBrandAllAgentRequest(id));

    [HttpGet("{id}/{allAgentGameType}/validBet")]
    [Description("Get the brand's all agent valid bet settings")]
    public IAsyncEnumerable<ValidBetConditionModel> GetBrandAllAgentValidBetConditionAsync(Guid id,
        AllAgentGameType allAgentGameType)
        => _mediator.CreateStream(new GetBrandAllAgentValidBetSettingRequest(id, allAgentGameType));

    [HttpPut("gameType")]
    [Description("Update the brand's all agent game type switch")]
    public Task<MessageResponse> UpdateBrandAllAgentGameTypeAsync(
        UpdateBrandAllAgentGameTypeRequest request)
        => _mediator.Send(request);

    [HttpPut("switch")]
    [Description("Update the brand's all agent switch")]
    public Task<MessageResponse> UpdateBrandAllAgentSwitchAsync(
        UpdateBrandAllAgentSwitchRequest request)
        => _mediator.Send(request);

    [HttpPatch("settlement-interval")]
    [Description("Update the brand's all agent settlement interval")]
    public Task<MessageResponse> UpdateBrandAllAgentSettlementIntervalAsync(
        UpdateBrandAllAgentSettleIntervalRequest request)
        => _mediator.Send(request);

    [HttpPut("validBet")]
    [Description("Update the brand's all agent valid bet conditions")]
    public Task<MessageResponse> UpdateBrandAllAgentValidBetConditionsAsync(
        UpdateBrandAllAgentValidBetConditionRequest request)
        => _mediator.Send(request);
}