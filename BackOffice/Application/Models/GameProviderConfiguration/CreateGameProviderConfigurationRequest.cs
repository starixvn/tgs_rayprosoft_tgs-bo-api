using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.GameProviderConfiguration;

public class CreateGameProviderConfigurationRequest : GameProviderConfigurationRequest, IRequest<MessageResponse>
{
}
