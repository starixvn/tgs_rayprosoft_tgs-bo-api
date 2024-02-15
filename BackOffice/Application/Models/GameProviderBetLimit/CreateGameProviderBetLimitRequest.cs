using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

public class CreateGameProviderBetLimitRequest : GameProviderBetLimitRequest, IRequest<MessageResponse>
{
}
