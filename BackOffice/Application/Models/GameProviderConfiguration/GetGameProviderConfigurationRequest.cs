using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.GameProviderConfiguration;

public class GetGameProviderConfigurationRequest : IRequest<GetGameProviderConfigurationResponse>
{
    public GetGameProviderConfigurationRequest(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; set; }
}