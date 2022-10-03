using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

public class CreateGameProviderBetLimitRequest : GameProviderBetLimitRequest, IRequest<MessageResponse>
{
}