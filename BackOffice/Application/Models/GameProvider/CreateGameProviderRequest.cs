using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.GameProvider;

public class CreateGameProviderRequest : GameProviderRequest, IRequest<MessageResponse>
{
}