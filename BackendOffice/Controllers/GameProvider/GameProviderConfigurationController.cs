using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.GameProviderConfiguration;

namespace TGS.Apis.BackendOffice.Controllers.GameProvider;

[Route("api/[controller]")]
[ApiController]
public class GameProviderConfigurationController : BaseContextController
{
    private readonly IMediator _mediator;

    public GameProviderConfigurationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Description("Get game provider configurations")]
    public Task<PagedResponse<GameProviderConfigurationModel>>
        GetGameProviderConfigurationsAsync([FromQuery] GetGameProviderConfigurationsRequest request) =>
        _mediator.Send(request);

    [HttpGet("{id}")]
    [Description("Get a game provider configuration")]
    public Task<GetGameProviderConfigurationResponse> GetGameProviderConfigurationAsync(Guid id) =>
        _mediator.Send(new GetGameProviderConfigurationRequest(id));

    [HttpPost]
    [Description("Create a game provider configuration")]
    public Task<MessageResponse> CreateGameProviderConfigurationAsync(
        CreateGameProviderConfigurationRequest request) =>
        _mediator.Send(request);

    [HttpPut("{id}")]
    [Description("Update a game provider configuration")]
    public Task<MessageResponse> UpdateGameProviderConfigurationAsync(Guid id,
        UpdateGameProviderConfigurationRequest request)
    {
        request.Id = id;
        return _mediator.Send(request);
    }
}