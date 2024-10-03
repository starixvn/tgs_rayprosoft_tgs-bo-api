using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Services.BackOffice.Application.Models.GameProviderConfiguration;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.GameProvider;

[Route("api/[controller]")]
[ApiController]
public class GameProviderConfigurationController(IMediator mediator) : BaseContextController
{
    [HttpPost]
    [BOAuthorize(["SystemAdmin.GameProviders.Update"])]
    [Description("Create a game provider configuration")]
    public Task<MessageResponse> CreateGameProviderConfigurationAsync(
        CreateGameProviderConfigurationRequest request) =>
        mediator.Send(request);

    [HttpGet("{id}")]
    [BOAuthorize(["SystemAdmin.GameProviders.Update"])]
    [Description("Get a game provider configuration")]
    public Task<GetGameProviderConfigurationResponse> GetGameProviderConfigurationAsync(Guid id) =>
        mediator.Send(new GetGameProviderConfigurationRequest(id));

    [HttpGet]
    [BOAuthorize(["SystemAdmin.GameProviders.Update", "Merchant.Brands", "Promotion.PromotionList"])]
    [Description("Get game provider configurations")]
    public Task<PagedResponse<GameProviderConfigurationModel>>
        GetGameProviderConfigurationsAsync([FromQuery] GetGameProviderConfigurationsRequest request) =>
        mediator.Send(request);

    [HttpPut("{id}")]
    [BOAuthorize(["SystemAdmin.GameProviders.Update"])]
    [Description("Update a game provider configuration")]
    public Task<MessageResponse> UpdateGameProviderConfigurationAsync(Guid id,
        UpdateGameProviderConfigurationRequest request)
    {
        request.Id = id;
        return mediator.Send(request);
    }
}