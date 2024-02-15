using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.BrandAllAgent;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Brand;

[Route("api/[controller]")]
[ApiController]
public class BrandAllAgentController(IMediator mediator) : BaseContextController
{
    [HttpGet("{id}")]
    [Description("Get the brand's all agent settings")]
    public Task<GetBrandAllAgentResponse> GetBrandAllAgentAsync(Guid id)
        => mediator.Send(new GetBrandAllAgentRequest(id));

    [HttpGet("{id}/{allAgentGameType}/validBet")]
    [Description("Get the brand's all agent valid bet settings")]
    public IAsyncEnumerable<ValidBetConditionModel> GetBrandAllAgentValidBetConditionAsync(Guid id,
        AllAgentGameType allAgentGameType)
        => mediator.CreateStream(new GetBrandAllAgentValidBetSettingRequest(id, allAgentGameType));

    [HttpPut("gameType")]
    [Description("Update the brand's all agent game type switch")]
    public Task<MessageResponse> UpdateBrandAllAgentGameTypeAsync(
        UpdateBrandAllAgentGameTypeRequest request)
        => mediator.Send(request);

    [HttpPut("switch")]
    [Description("Update the brand's all agent switch")]
    public Task<MessageResponse> UpdateBrandAllAgentSwitchAsync(
        UpdateBrandAllAgentSwitchRequest request)
        => mediator.Send(request);

    [HttpPatch("settlement-interval")]
    [Description("Update the brand's all agent settlement interval")]
    public Task<MessageResponse> UpdateBrandAllAgentSettlementIntervalAsync(
        UpdateBrandAllAgentSettleIntervalRequest request)
        => mediator.Send(request);

    [HttpPut("validBet")]
    [Description("Update the brand's all agent valid bet conditions")]
    public Task<MessageResponse> UpdateBrandAllAgentValidBetConditionsAsync(
        UpdateBrandAllAgentValidBetConditionRequest request)
        => mediator.Send(request);
}