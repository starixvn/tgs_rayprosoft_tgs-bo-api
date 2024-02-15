using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.GameProvider;

public class CreateGameProviderRequest : GameProviderRequest, IRequest<MessageResponse>
{
}
