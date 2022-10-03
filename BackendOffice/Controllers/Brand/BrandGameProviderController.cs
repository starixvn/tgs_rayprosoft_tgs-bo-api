using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;

namespace TGS.Apis.BackendOffice.Controllers.Brand;

[Route("api/[controller]")]
[ApiController]
public partial class BrandGameProviderController : BaseContextController
{
    private readonly IMediator _mediator;

    public BrandGameProviderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Description("Get brand game providers")]
    public Task<PagedResponse<BrandGameProviderModel>> GetBrandGameProvidersAsync(
        [FromQuery] GetBrandGameProvidersRequest request) =>
        _mediator.Send(request);

    [HttpGet("{brandGameProviderId}/defaultTimes")]
    [Description("Get default times")]
    public Task<GetDefaultTimesResponse> GetBrandGameProviderDefaultTimesAsync(Guid brandGameProviderId) =>
        _mediator.Send(new GetDefaultTimesRequest(brandGameProviderId));

    [HttpPut("{id}")]
    [Description("Update a brand game provider")]
    public Task<MessageResponse> UpdateBrandGameProviderAsync(Guid id,
        UpdateBrandGameProviderRequest request)
    {
        request.Id = id;
        return _mediator.Send(request);
    }

    [HttpPut("{brandGameProviderId}/defaultTimes")]
    [Description("Update default times")]
    public Task<MessageResponse> UpdateBrandGameProviderDefaultTimesAsync(Guid brandGameProviderId,
        UpdateDefaultTimesRequest request)
    {
        request.BrandGameProviderId = brandGameProviderId;
        return _mediator.Send(request);
    }
}