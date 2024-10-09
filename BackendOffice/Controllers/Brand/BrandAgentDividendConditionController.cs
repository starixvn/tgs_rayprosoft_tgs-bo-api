using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Apis.BackendOffice.Filters;
using TGS.Services.BackOffice.Application.Models.BrandAgentDividend;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Brand;

[Route("api/[controller]")]
[ApiController]
public class BrandAgentDividendConditionController(IMediator mediator) : BaseContextController
{
    [BOTypeFilter(typeof(MerchanPermissionRouteFiler), Arguments = ["brand", "id"])]
    [HttpGet("{id}")]
    [Description("Get brand agent dividend conditions")]
    public Task<PagedResponse<BrandAgentDividendConditionModel>> GetBrandAsync(Guid id)
        => mediator.Send(new GetBrandAgentDividendConditionsRequest(id));

    [BOTypeFilter(typeof(MerchanPermissionBodyFiler), Arguments = ["brand", "BrandId"])]
    [HttpPut]
    [Description("Update the brand agent dividend conditions")]
    public Task<MessageResponse> UpdateBrandAgentDividendConditionsAsync(
        UpdateBrandAgentDividendConditionRequest request)
        => mediator.Send(request);
}