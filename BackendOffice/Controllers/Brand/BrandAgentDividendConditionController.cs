using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.BrandAgentDividend;

namespace TGS.Apis.BackendOffice.Controllers.Brand;

[Route("api/[controller]")]
[ApiController]
public class BrandAgentDividendConditionController : BaseContextController
{
    private readonly IMediator _mediator;

    public BrandAgentDividendConditionController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id}")]
    [Description("Get brand agent dividend conditions")]
    public Task<PagedResponse<BrandAgentDividendConditionModel>> GetBrandAsync(Guid id)
        => _mediator.Send(new GetBrandAgentDividendConditionsRequest(id));

    [HttpPut]
    [Description("Update the brand agent dividend conditions")]
    public Task<MessageResponse> UpdateBrandAgentDividendConditionsAsync(
        UpdateBrandAgentDividendConditionRequest request)
        => _mediator.Send(request);
}