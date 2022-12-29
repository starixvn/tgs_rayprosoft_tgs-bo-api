using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.GameProviderConfiguration;

public class UpdateGameProviderConfigurationRequest : GameProviderConfigurationRequest, IRequest<MessageResponse>
{
}
