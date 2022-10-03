using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.GameProviderConfiguration;

public class GetGameProviderConfigurationsRequest : PageableRequest,
    IRequest<PagedResponse<GameProviderConfigurationModel>>
{
    public Guid GameProviderId { get; set; }
}