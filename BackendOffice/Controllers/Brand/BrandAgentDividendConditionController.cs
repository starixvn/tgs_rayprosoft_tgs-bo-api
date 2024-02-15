using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TGS.Services.BackOffice.Application.Models.BrandAgentDividend;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Brand;

[Route("api/[controller]")]
[ApiController]
public class BrandAgentDividendConditionController(IMediator mediator) : BaseContextController
{
    [HttpGet("{id}")]
    [Description("Get brand agent dividend conditions")]
    public Task<PagedResponse<BrandAgentDividendConditionModel>> GetBrandAsync(Guid id)
        => mediator.Send(new GetBrandAgentDividendConditionsRequest(id));

    [HttpPut]
    [Description("Update the brand agent dividend conditions")]
    public Task<MessageResponse> UpdateBrandAgentDividendConditionsAsync(
        UpdateBrandAgentDividendConditionRequest request)
        => mediator.Send(request);
}
