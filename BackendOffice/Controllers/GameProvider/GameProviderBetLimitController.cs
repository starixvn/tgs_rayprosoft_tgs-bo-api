using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.GameProvider;
using TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

namespace TGS.Apis.BackendOffice.Controllers.GameProvider;

[Route("api/[controller]")]
[ApiController]
public class GameProviderBetLimitController : BaseContextController
{
    private readonly IMediator _mediator;

    public GameProviderBetLimitController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Description("Get game provider bet limits")]
    public Task<GetGameProviderBetLimitsResponse>
        GetGameProviderBetLimitsAsync([FromQuery] GetGameProviderBetLimitsRequest request) =>
        _mediator.Send(request);

    [HttpGet("{id}")]
    [Description("Get a game provider bet limit")]
    public Task<GetGameProviderBetLimitResponse> GetGameProviderBetLimitAsync(Guid id) =>
        _mediator.Send(new GetGameProviderBetLimitRequest(id));

    [HttpGet("{id}/InUse")]
    [Description("Get in use brand names")]
    public Task<GetInUseBrandsResponse> GetInUseBrandNamesAsync(Guid id) =>
        _mediator.Send(new GetInUseBrandsRequest(id));

    [HttpPost]
    [Description("Create a game provider bet limit")]
    public Task<MessageResponse> CreateGameProviderBetLimitAsync(CreateGameProviderBetLimitRequest request) =>
        _mediator.Send(request);

    [HttpPut("{id}")]
    [Description("Update a game provider bet limit")]
    public Task<MessageResponse> UpdateGameProviderBetLimitAsync(Guid id, UpdateGameProviderBetLimitRequest request)
    {
        request.Id = id;
        return _mediator.Send(request);
    }
}