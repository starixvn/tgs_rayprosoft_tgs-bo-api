using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.GameProviderConfiguration;

public class GetGameProviderConfigurationRequest(Guid id) : IRequest<GetGameProviderConfigurationResponse>
{
    public Guid Id { get; set; } = id;
}
