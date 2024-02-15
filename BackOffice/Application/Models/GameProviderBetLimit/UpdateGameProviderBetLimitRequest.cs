using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

public class UpdateGameProviderBetLimitRequest : GameProviderBetLimitRequest, IRequest<MessageResponse>
{
}
