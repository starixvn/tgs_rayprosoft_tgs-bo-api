using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

namespace TGS.Apis.BackendOffice.Controllers.GameProvider;

[Route("api/[controller]")]
[ApiController]
public class GameProviderBetLimitController(IMediator mediator) : BaseContextController
{
    [HttpPost]
    [Description("Create a game provider bet limit")]
    public Task<MessageResponse> CreateGameProviderBetLimitAsync(CreateGameProviderBetLimitRequest request) =>
        mediator.Send(request);

    [HttpGet("{id}")]
    [Description("Get a game provider bet limit")]
    public Task<GetGameProviderBetLimitResponse> GetGameProviderBetLimitAsync(Guid id) =>
        mediator.Send(new GetGameProviderBetLimitRequest(id));

    [HttpGet]
    [Description("Get game provider bet limits")]
    public Task<GetGameProviderBetLimitsResponse>
        GetGameProviderBetLimitsAsync([FromQuery] GetGameProviderBetLimitsRequest request) =>
        mediator.Send(request);

    [HttpGet("{id}/InUse")]
    [Description("Get in use brand names")]
    public Task<GetInUseBrandsResponse> GetInUseBrandNamesAsync(Guid id) =>
        mediator.Send(new GetInUseBrandsRequest(id));

    [HttpPut("{id}")]
    [Description("Update a game provider bet limit")]
    public Task<MessageResponse> UpdateGameProviderBetLimitAsync(Guid id, UpdateGameProviderBetLimitRequest request)
    {
        request.Id = id;
        return mediator.Send(request);
    }
}
