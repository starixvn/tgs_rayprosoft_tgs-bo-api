using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.GameProviderConfiguration;

public class CreateGameProviderConfigurationRequest : GameProviderConfigurationRequest, IRequest<MessageResponse>
{
}