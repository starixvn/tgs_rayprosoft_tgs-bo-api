using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Brand;

[Route("api/[controller]")]
[ApiController]
public partial class BrandGameProviderController(IMediator mediator) : BaseContextController
{
    [HttpGet("{brandGameProviderId}/defaultTimes")]
    [Description("Get default times")]
    public Task<GetDefaultTimesResponse> GetBrandGameProviderDefaultTimesAsync(Guid brandGameProviderId) =>
        mediator.Send(new GetDefaultTimesRequest(brandGameProviderId));

    [HttpGet]
    [Description("Get brand game providers")]
    public Task<PagedResponse<BrandGameProviderModel>> GetBrandGameProvidersAsync(
        [FromQuery] GetBrandGameProvidersRequest request) =>
        mediator.Send(request);

    [HttpPut("{id}")]
    [Description("Update a brand game provider")]
    public Task<MessageResponse> UpdateBrandGameProviderAsync(Guid id,
        UpdateBrandGameProviderRequest request)
    {
        request.Id = id;
        return mediator.Send(request);
    }

    [HttpPut("{brandGameProviderId}/defaultTimes")]
    [Description("Update default times")]
    public Task<MessageResponse> UpdateBrandGameProviderDefaultTimesAsync(Guid brandGameProviderId,
        UpdateDefaultTimesRequest request)
    {
        request.BrandGameProviderId = brandGameProviderId;
        return mediator.Send(request);
    }
}
